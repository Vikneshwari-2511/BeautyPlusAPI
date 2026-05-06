using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Data;
using BeautyPlusParlour.Exceptions;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Notification;
using BeautyPlusParlour.Models.Entities;
using BeautyPlusParlour.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BeautyPlusParlour.Services;

public sealed class NotificationService : INotificationService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        AppDbContext db,
        IEmailService email,
        ILogger<NotificationService> logger)
    {
        _db = db;
        _email = email;
        _logger = logger;
    }

    // ── In-app CRUD ───────────────────────────────────────────────────────

    public async Task<IReadOnlyList<NotificationDto>> GetAllAsync(
        Guid userId, CancellationToken ct = default)
    {
        var notifications = await _db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);

        return notifications.Select(MapToDto).ToList().AsReadOnly();
    }

    public async Task<UnreadCountDto> GetUnreadCountAsync(
        Guid userId, CancellationToken ct = default)
    {
        var count = await _db.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead, ct);

        return new UnreadCountDto(count);
    }

    public async Task MarkReadAsync(
        Guid notificationId, Guid userId,
        CancellationToken ct = default)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n =>
                n.Id == notificationId &&
                n.UserId == userId, ct)
            ?? throw new NotFoundException(ResponseMessages.NotificationNotFound);

        if (!notification.IsRead)
        {
            notification.MarkRead();
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task MarkAllReadAsync(
        Guid userId, CancellationToken ct = default)
    {
        var unread = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(ct);

        foreach (var n in unread)
            n.MarkRead();

        if (unread.Count > 0)
            await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(
        Guid notificationId, Guid userId,
        CancellationToken ct = default)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n =>
                n.Id == notificationId &&
                n.UserId == userId, ct)
            ?? throw new NotFoundException(ResponseMessages.NotificationNotFound);

        _db.Notifications.Remove(notification);
        await _db.SaveChangesAsync(ct);
    }

    // ── Booking triggers ──────────────────────────────────────────────────

    public async Task NotifyBookingCreatedAsync(
        Guid bookingId, CancellationToken ct = default)
    {
        var booking = await LoadBookingAsync(bookingId, ct);
        if (booking is null) return;

        // Notify customer
        await CreateAsync(
            booking.Customer.UserId,
            "Booking Received!",
            $"Your booking {booking.BookingCode} for {booking.Items.First().ServiceName} " +
            $"on {booking.BookingDate:dd MMM yyyy} at {booking.BookingTime:hh\\:mm tt} has been received. " +
            $"We will confirm it shortly.",
            NotificationType.BookingCreated,
            booking.Id, ReferenceType.Booking, ct);

        // Notify staff
        await CreateAsync(
            booking.Staff.UserId,
            "New Booking Assigned",
            $"You have a new booking {booking.BookingCode} on " +
            $"{booking.BookingDate:dd MMM yyyy} at {booking.BookingTime:hh\\:mm tt}. " +
            $"Service: {booking.Items.First().ServiceName}.",
            NotificationType.BookingCreated,
            booking.Id, ReferenceType.Booking, ct);

        // Email customer
        await SendBookingEmailAsync(
            booking.Customer.User.Email,
            booking.Customer.FullName,
            "Booking Received",
            $"Your booking <strong>{booking.BookingCode}</strong> has been received. " +
            $"We'll confirm it shortly.",
            ct);

        _logger.LogInformation(
            "Booking created notifications sent for {Code}",
            booking.BookingCode);
    }

    public async Task NotifyBookingConfirmedAsync(
        Guid bookingId, CancellationToken ct = default)
    {
        var booking = await LoadBookingAsync(bookingId, ct);
        if (booking is null) return;

        await CreateAsync(
            booking.Customer.UserId,
            "Booking Confirmed ✓",
            $"Great news! Your booking {booking.BookingCode} on " +
            $"{booking.BookingDate:dd MMM yyyy} at {booking.BookingTime:hh\\:mm tt} is confirmed.",
            NotificationType.BookingConfirmed,
            booking.Id, ReferenceType.Booking, ct);

        await SendBookingEmailAsync(
            booking.Customer.User.Email,
            booking.Customer.FullName,
            "Booking Confirmed",
            $"Your booking <strong>{booking.BookingCode}</strong> on " +
            $"<strong>{booking.BookingDate:dd MMM yyyy}</strong> at " +
            $"<strong>{booking.BookingTime:hh\\:mm tt}</strong> is confirmed. " +
            $"Your stylist will be <strong>{booking.Staff.FullName}</strong>.",
            ct);
    }

    public async Task NotifyBookingStartedAsync(
        Guid bookingId, CancellationToken ct = default)
    {
        var booking = await LoadBookingAsync(bookingId, ct);
        if (booking is null) return;

        await CreateAsync(
            booking.Customer.UserId,
            "Your Service Has Started",
            $"Your booking {booking.BookingCode} is now in progress. " +
            $"{booking.Staff.FullName} has started your service.",
            NotificationType.BookingStarted,
            booking.Id, ReferenceType.Booking, ct);
    }

    public async Task NotifyBookingCompletedAsync(
        Guid bookingId, CancellationToken ct = default)
    {
        var booking = await LoadBookingAsync(bookingId, ct);
        if (booking is null) return;

        await CreateAsync(
            booking.Customer.UserId,
            "Service Completed — Thank You!",
            $"Your booking {booking.BookingCode} is complete. " +
            $"You earned {booking.LoyaltyPointsEarned} loyalty points. " +
            $"We'd love your feedback!",
            NotificationType.BookingCompleted,
            booking.Id, ReferenceType.Booking, ct);

        await SendBookingEmailAsync(
            booking.Customer.User.Email,
            booking.Customer.FullName,
            "Thank You for Your Visit",
            $"Your service is complete. You earned <strong>{booking.LoyaltyPointsEarned} loyalty points</strong>. " +
            $"Please take a moment to rate your experience!",
            ct);
    }

    public async Task NotifyBookingCancelledAsync(
        Guid bookingId, CancellationToken ct = default)
    {
        var booking = await LoadBookingAsync(bookingId, ct);
        if (booking is null) return;

        // Notify customer
        await CreateAsync(
            booking.Customer.UserId,
            "Booking Cancelled",
            $"Your booking {booking.BookingCode} on " +
            $"{booking.BookingDate:dd MMM yyyy} has been cancelled.",
            NotificationType.BookingCancelled,
            booking.Id, ReferenceType.Booking, ct);

        // Notify staff
        await CreateAsync(
            booking.Staff.UserId,
            "Booking Cancelled",
            $"Booking {booking.BookingCode} on " +
            $"{booking.BookingDate:dd MMM yyyy} has been cancelled.",
            NotificationType.BookingCancelled,
            booking.Id, ReferenceType.Booking, ct);

        await SendBookingEmailAsync(
            booking.Customer.User.Email,
            booking.Customer.FullName,
            "Booking Cancelled",
            $"Your booking <strong>{booking.BookingCode}</strong> has been cancelled.",
            ct);
    }

    public async Task NotifyBookingRescheduledAsync(
        Guid bookingId, CancellationToken ct = default)
    {
        var booking = await LoadBookingAsync(bookingId, ct);
        if (booking is null) return;

        await CreateAsync(
            booking.Customer.UserId,
            "Booking Rescheduled",
            $"Your booking {booking.BookingCode} has been rescheduled to " +
            $"{booking.BookingDate:dd MMM yyyy} at {booking.BookingTime:hh\\:mm tt}.",
            NotificationType.BookingRescheduled,
            booking.Id, ReferenceType.Booking, ct);

        await CreateAsync(
            booking.Staff.UserId,
            "Booking Rescheduled",
            $"Booking {booking.BookingCode} has been rescheduled to " +
            $"{booking.BookingDate:dd MMM yyyy} at {booking.BookingTime:hh\\:mm tt}.",
            NotificationType.BookingRescheduled,
            booking.Id, ReferenceType.Booking, ct);

        await SendBookingEmailAsync(
            booking.Customer.User.Email,
            booking.Customer.FullName,
            "Booking Rescheduled",
            $"Your booking <strong>{booking.BookingCode}</strong> has been rescheduled to " +
            $"<strong>{booking.BookingDate:dd MMM yyyy}</strong> at " +
            $"<strong>{booking.BookingTime:hh\\:mm tt}</strong>.",
            ct);
    }

    // ── Leave triggers ────────────────────────────────────────────────────

    public async Task NotifyLeaveApprovedAsync(
        Guid leaveId, CancellationToken ct = default)
    {
        var leave = await LoadLeaveAsync(leaveId, ct);
        if (leave is null) return;

        await CreateAsync(
            leave.Staff.UserId,
            "Leave Approved ✓",
            $"Your leave request from {leave.LeaveFromDate:dd MMM yyyy} " +
            $"to {leave.LeaveToDate:dd MMM yyyy} ({leave.TotalDays} day(s)) has been approved.",
            NotificationType.LeaveApproved,
            leave.Id, ReferenceType.Leave, ct);
    }

    public async Task NotifyLeaveRejectedAsync(
        Guid leaveId, CancellationToken ct = default)
    {
        var leave = await LoadLeaveAsync(leaveId, ct);
        if (leave is null) return;

        var reason = string.IsNullOrWhiteSpace(leave.RejectionReason)
            ? string.Empty
            : $" Reason: {leave.RejectionReason}";

        await CreateAsync(
            leave.Staff.UserId,
            "Leave Request Rejected",
            $"Your leave request from {leave.LeaveFromDate:dd MMM yyyy} " +
            $"to {leave.LeaveToDate:dd MMM yyyy} has been rejected.{reason}",
            NotificationType.LeaveRejected,
            leave.Id, ReferenceType.Leave, ct);
    }

    // ── Loyalty trigger ───────────────────────────────────────────────────

    public async Task NotifyLoyaltyEarnedAsync(
        Guid customerId, int points,
        string tier, CancellationToken ct = default)
    {
        var customer = await _db.CustomerProfiles
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == customerId, ct);

        if (customer is null) return;

        await CreateAsync(
            customer.UserId,
            $"You Earned {points} Loyalty Points!",
            $"You now have loyalty points on your account. " +
            $"Your current tier is {tier}. " +
            $"Keep booking to unlock better discounts!",
            NotificationType.LoyaltyEarned,
            customerId, ReferenceType.Loyalty, ct);
    }

    // ── private ───────────────────────────────────────────────────────────

    private async Task CreateAsync(
        Guid userId, string title, string body,
        string type, Guid? referenceId,
        string? referenceType,
        CancellationToken ct)
    {
        var notification = Notification.Create(
            userId, title, body,
            type, referenceId, referenceType);

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(ct);
    }

    private async Task<Booking?> LoadBookingAsync(
        Guid bookingId, CancellationToken ct)
    {
        return await _db.Bookings
            .AsNoTracking()
            .Include(b => b.Customer)
                .ThenInclude(c => c.User)
            .Include(b => b.Staff)
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.Id == bookingId, ct);
    }

    private async Task<StaffLeave?> LoadLeaveAsync(
        Guid leaveId, CancellationToken ct)
    {
        return await _db.StaffLeaves
            .AsNoTracking()
            .Include(l => l.Staff)
            .FirstOrDefaultAsync(l => l.Id == leaveId, ct);
    }

    private async Task SendBookingEmailAsync(
        string toEmail, string fullName,
        string subject, string bodyHtml,
        CancellationToken ct)
    {
        try
        {
            await _email.SendGeneralAsync(
                toEmail, fullName, subject, bodyHtml, ct);
        }
        catch (Exception ex)
        {
            // Email failure should never crash the main flow
            _logger.LogWarning(ex,
                "Failed to send notification email to {Email}", toEmail);
        }
    }

    private static NotificationDto MapToDto(Notification n) =>
        new(n.Id, n.Title, n.Body, n.Type,
            n.ReferenceId, n.ReferenceType,
            n.IsRead, n.ReadAt, n.CreatedAt);
}