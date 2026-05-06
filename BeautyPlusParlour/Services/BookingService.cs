using System.Text.Json;
using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Data;
using BeautyPlusParlour.Exceptions;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Booking;
using BeautyPlusParlour.Models.DTOs.Common;
using BeautyPlusParlour.Models.Entities;
using BeautyPlusParlour.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BeautyPlusParlour.Services;

public sealed class BookingService : IBookingService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly ILogger<BookingService> _logger;
    private readonly ILoyaltyService _loyalty;
    // ADD to BookingService fields
    private readonly INotificationService _notifications;
    public BookingService(
        AppDbContext db,
        IAuditService audit,
        ILogger<BookingService> logger,
        INotificationService notifications,
        ILoyaltyService loyalty)
    {
        _db = db;
        _audit = audit;
        _logger = logger;
        _notifications = notifications;
        _loyalty = loyalty;
    }

    // ── Create ────────────────────────────────────────────────────────────
    public async Task<BookingDto> CreateAsync(
        Guid userId, CreateBookingRequest request,
        CancellationToken ct = default)
    {
        // 1. Get customer profile
        var customer = await _db.CustomerProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive, ct)
            ?? throw new NotFoundException(ResponseMessages.CustomerNotFound);

        // 2. Get staff
        var staff = await _db.StaffProfiles
            .FirstOrDefaultAsync(s => s.Id == request.StaffId && s.IsActive, ct)
            ?? throw new NotFoundException(ResponseMessages.StaffNotFound);

        // 3. Load services and validate
        var services = await _db.Services
            .Where(s => request.ServiceIds.Contains(s.Id) && s.IsActive)
            .ToListAsync(ct);

        if (services.Count != request.ServiceIds.Count)
            throw new NotFoundException(ResponseMessages.ServiceNotFound);

        // 4. Validate staff has all skills
        foreach (var service in services)
        {
            var hasSkill = await _db.StaffSkills
                .AnyAsync(sk =>
                    sk.StaffId == request.StaffId &&
                    sk.ServiceId == service.Id &&
                    sk.IsActive, ct);

            if (!hasSkill)
                throw new AppException(ResponseMessages.StaffSkillNotFound);
        }

        // 5. Determine booking type
        var hasOnSite = services.Any(s =>
            s.ServiceTypeActual == ServiceType.OnSite ||
            s.ServiceTypeActual == ServiceType.Both);

        var bookingType = hasOnSite
            ? BookingType.OnSite
            : BookingType.Parlour;

        // 6. OnSite requires address
        if (bookingType == BookingType.OnSite && !request.AddressId.HasValue)
            throw new AppException(ResponseMessages.BookingAddressRequired);

        if (request.AddressId.HasValue)
        {
            var addrExists = await _db.CustomerAddresses
                .AnyAsync(a =>
                    a.Id == request.AddressId &&
                    a.CustomerId == customer.Id &&
                    a.IsActive, ct);

            if (!addrExists)
                throw new NotFoundException(ResponseMessages.AddressNotFound);
        }

        // 7. Check staff not on leave
        var onLeave = await _db.StaffLeaves
            .AnyAsync(l =>
                l.StaffId == request.StaffId &&
                l.Status == LeaveStatus.Approved &&
                l.LeaveFromDate <= request.BookingDate &&
                l.LeaveToDate >= request.BookingDate, ct);

        if (onLeave)
            throw new AppException("Selected staff is on leave on this date.");

        // 8. Check staff schedule
        var dayOfWeek = (int)request.BookingDate.DayOfWeek;
        var schedule = await _db.StaffSchedules
            .FirstOrDefaultAsync(sc =>
                sc.StaffId == request.StaffId &&
                sc.DayOfWeek == dayOfWeek &&
                sc.IsWorkingDay, ct)
            ?? throw new AppException("Selected staff is not working on this day.");

        // 9. Calculate total slot time
        var totalDuration = services.Sum(s =>
            s.DurationMinutes + s.BufferMinutes);
        var endTime = request.BookingTime.AddMinutes(totalDuration);

        if (request.BookingTime < schedule.StartTime ||
            endTime > schedule.EndTime)
            throw new AppException(
                $"Booking time must be between {schedule.StartTime} and {schedule.EndTime}.");

        // 10. Check slot conflict
        var conflict = await HasSlotConflictAsync(
            request.StaffId, request.BookingDate,
            request.BookingTime, endTime, null, ct);

        if (conflict)
            throw new AppException(ResponseMessages.SlotNotAvailable);

        // 11. Calculate amounts
        var totalAmount = services.Sum(s =>
            s.DiscountedPrice ?? s.BasePrice);

        var travelCharge = 0m;
        if (bookingType == BookingType.OnSite)
        {
            var onSiteDetail = await _db.OnSiteDetails
                .FirstOrDefaultAsync(o =>
                    services.Select(s => s.Id).Contains(o.ServiceId), ct);
            travelCharge = onSiteDetail?.TravelCharge ?? 0;
        }

        var advanceAmount = bookingType == BookingType.OnSite
            ? Math.Round(totalAmount * 0.30m, 2)
            : 0m;

        var requiresConsultation = services.Any(s => s.RequiresConsultation);

        // 12. Calculate loyalty points earned
        var totalPoints = services.Sum(s => s.LoyaltyPoints);

        // 13. Generate booking code
        var code = await GenerateBookingCodeAsync(ct);

        // 14. Create booking
        var booking = Booking.Create(
            code, customer.Id, request.StaffId,
            request.AddressId, request.BookingDate,
            request.BookingTime, bookingType,
            totalAmount, 0m, travelCharge,
            advanceAmount, request.CouponCode,
            request.LoyaltyPointsToUse,
            request.Notes, requiresConsultation);

        _db.Bookings.Add(booking);

        // 15. Create booking items (snapshot prices)
        foreach (var service in services)
        {
            var item = BookingItem.Create(
                booking.Id, service.Id,
                service.Name,
                service.DiscountedPrice ?? service.BasePrice,
                service.DurationMinutes, service.BufferMinutes,
                service.LoyaltyPoints);

            _db.BookingItems.Add(item);
        }

        await _db.SaveChangesAsync(ct);
        await _notifications.NotifyBookingCreatedAsync(booking.Id, ct);
        await _audit.LogAsync(
            userId, AuditAction.Created,
            "Booking", booking.Id.ToString(),
            null, JsonSerializer.Serialize(new
            {
                booking.BookingCode,
                booking.BookingDate,
                booking.FinalAmount
            }), ct: ct);

        _logger.LogInformation(
            "Booking created: {Code} for CustomerId {CustomerId}",
            booking.BookingCode, customer.Id);

        return await GetByIdInternalAsync(booking.Id, ct);
    }

    // ── Get by ID ─────────────────────────────────────────────────────────
    public async Task<BookingDto> GetByIdAsync(
        Guid bookingId, Guid userId, bool isAdmin,
        CancellationToken ct = default)
    {
        var booking = await _db.Bookings
            .Include(b => b.Customer)
            .FirstOrDefaultAsync(b => b.Id == bookingId, ct)
            ?? throw new NotFoundException(ResponseMessages.BookingNotFound);

        // Customers can only see their own bookings
        if (!isAdmin && booking.Customer.UserId != userId)
            throw new UnauthorizedException("Access denied.");

        return await GetByIdInternalAsync(bookingId, ct);
    }

    // ── Get all (Admin) ───────────────────────────────────────────────────
    public async Task<PagedResponse<BookingListDto>> GetAllAsync(
        BookingStatus? status, DateOnly? date,
        int page, int pageSize,
        CancellationToken ct = default)
    {
        pageSize = Math.Min(pageSize, 50);

        var query = _db.Bookings
            .AsNoTracking()
            .Include(b => b.Customer)
            .Include(b => b.Staff)
            .Include(b => b.Items)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(b => b.Status == status);

        if (date.HasValue)
            query = query.Where(b => b.BookingDate == date);

        var total = await query.CountAsync(ct);

        var bookings = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResponse<BookingListDto>(
            bookings.Select(MapToListDto).ToList(),
            total, page, pageSize);
    }

    // ── Get my bookings (Customer) ────────────────────────────────────────
    public async Task<IReadOnlyList<BookingListDto>> GetMyBookingsAsync(
        Guid userId, CancellationToken ct = default)
    {
        var customer = await _db.CustomerProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId, ct)
            ?? throw new NotFoundException(ResponseMessages.CustomerNotFound);

        var bookings = await _db.Bookings
            .AsNoTracking()
            .Include(b => b.Customer)
            .Include(b => b.Staff)
            .Include(b => b.Items)
            .Where(b => b.CustomerId == customer.Id)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync(ct);

        return bookings.Select(MapToListDto).ToList().AsReadOnly();
    }

    // ── Confirm ───────────────────────────────────────────────────────────
    public async Task<BookingDto> ConfirmAsync(
        Guid bookingId, Guid adminId,
        CancellationToken ct = default)
    {
        var booking = await _db.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId, ct)
            ?? throw new NotFoundException(ResponseMessages.BookingNotFound);

        if (booking.Status != BookingStatus.Pending)
            throw new AppException("Only pending bookings can be confirmed.");

        // OnSite requires advance payment first
        if (booking.BookingType == BookingType.OnSite &&
            !booking.AdvancePaid &&
            booking.AdvanceAmount > 0)
            throw new AppException(ResponseMessages.AdvancePaymentRequired);

        booking.Confirm();
        await _db.SaveChangesAsync(ct);

        await _notifications.NotifyBookingConfirmedAsync(bookingId, ct);
        await _audit.LogAsync(
            adminId, AuditAction.Updated,
            "Booking", bookingId.ToString(),
            null, "Confirmed", ct: ct);

        return await GetByIdInternalAsync(bookingId, ct);
    }

    // ── Start ─────────────────────────────────────────────────────────────
    public async Task<BookingDto> StartAsync(
        Guid bookingId, Guid staffUserId,
        CancellationToken ct = default)
    {
        var booking = await GetBookingForStaffAsync(
            bookingId, staffUserId, ct);

        if (booking.Status != BookingStatus.Confirmed)
            throw new AppException("Only confirmed bookings can be started.");

        booking.Start();
        await _db.SaveChangesAsync(ct);
        await _notifications.NotifyBookingStartedAsync(bookingId, ct);
        return await GetByIdInternalAsync(bookingId, ct);
    }

    // ── Complete ──────────────────────────────────────────────────────────
    public async Task<BookingDto> CompleteAsync(
    Guid bookingId, Guid staffUserId,
    CancellationToken ct = default)
    {
        var booking = await GetBookingForStaffAsync(bookingId, staffUserId, ct);

        if (booking.Status != BookingStatus.InProgress)
            throw new AppException("Only in-progress bookings can be completed.");

        var pointsEarned = booking.CalculateTotalPoints();
        booking.Complete(pointsEarned);
        await _db.SaveChangesAsync(ct);
        await _notifications.NotifyBookingCompletedAsync(bookingId, ct);
        // ✅ Credit loyalty points automatically on completion
        if (pointsEarned > 0)
        {
            await _loyalty.CreditEarnedPointsAsync(
                booking.CustomerId,
                booking.Id,
                pointsEarned,
                $"Earned for booking {booking.BookingCode}",
                ct);
        }

        _logger.LogInformation(
            "Booking completed: {Code}. Points earned: {Points}",
            booking.BookingCode, pointsEarned);

        return await GetByIdInternalAsync(bookingId, ct);
    }
    // ── Cancel ────────────────────────────────────────────────────────────
    public async Task<BookingDto> CancelAsync(
        Guid bookingId, Guid userId, bool isAdmin,
        CancelBookingRequest request,
        CancellationToken ct = default)
    {
        var booking = await _db.Bookings
            .Include(b => b.Customer)
            .FirstOrDefaultAsync(b => b.Id == bookingId, ct)
            ?? throw new NotFoundException(ResponseMessages.BookingNotFound);

        if (!isAdmin && booking.Customer.UserId != userId)
            throw new UnauthorizedException("Access denied.");

        if (!booking.CanBeCancelled())
            throw new AppException(ResponseMessages.BookingCannotBeCancelled);

        booking.Cancel(userId, request.Reason);
        await _db.SaveChangesAsync(ct);
        await _notifications.NotifyBookingCancelledAsync(bookingId, ct);
        await _audit.LogAsync(
            userId, AuditAction.Updated,
            "Booking", bookingId.ToString(),
            null, $"Cancelled: {request.Reason}", ct: ct);

        return await GetByIdInternalAsync(bookingId, ct);
    }

    // ── Reschedule ────────────────────────────────────────────────────────
    public async Task<BookingDto> RescheduleAsync(
        Guid bookingId, Guid userId,
        RescheduleBookingRequest request,
        CancellationToken ct = default)
    {
        var booking = await _db.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.Id == bookingId, ct)
            ?? throw new NotFoundException(ResponseMessages.BookingNotFound);

        if (booking.Customer.UserId != userId)
            throw new UnauthorizedException("Access denied.");

        if (!booking.CanBeRescheduled())
            throw new AppException(ResponseMessages.BookingCannotReschedule);

        // Validate new slot
        var totalDuration = booking.Items
            .Sum(i => i.DurationMinutes + i.BufferMinutes);
        var newEndTime = request.NewTime.AddMinutes(totalDuration);

        var conflict = await HasSlotConflictAsync(
            request.StaffId, request.NewDate,
            request.NewTime, newEndTime, bookingId, ct);

        if (conflict)
            throw new AppException(ResponseMessages.SlotNotAvailable);

        booking.Reschedule(request.NewDate, request.NewTime, request.StaffId);
        await _db.SaveChangesAsync(ct);
        await _notifications.NotifyBookingRescheduledAsync(bookingId, ct);
        await _audit.LogAsync(
            userId, AuditAction.Updated,
            "Booking", bookingId.ToString(),
            null, $"Rescheduled to {request.NewDate} {request.NewTime}",
            ct: ct);

        return await GetByIdInternalAsync(bookingId, ct);
    }

    // ── Available Slots ───────────────────────────────────────────────────
    public async Task<IReadOnlyList<AvailableSlotDto>> GetAvailableSlotsAsync(
        AvailableSlotsRequest request,
        CancellationToken ct = default)
    {
        var service = await _db.Services
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId && s.IsActive, ct)
            ?? throw new NotFoundException(ResponseMessages.ServiceNotFound);

        var dayOfWeek = (int)request.Date.DayOfWeek;
        var schedule = await _db.StaffSchedules
            .FirstOrDefaultAsync(sc =>
                sc.StaffId == request.StaffId &&
                sc.DayOfWeek == dayOfWeek &&
                sc.IsWorkingDay, ct);

        if (schedule is null)
            return Array.Empty<AvailableSlotDto>();

        // Check leave
        var onLeave = await _db.StaffLeaves
            .AnyAsync(l =>
                l.StaffId == request.StaffId &&
                l.Status == LeaveStatus.Approved &&
                l.LeaveFromDate <= request.Date &&
                l.LeaveToDate >= request.Date, ct);

        if (onLeave)
            return Array.Empty<AvailableSlotDto>();

        var slotDuration = service.DurationMinutes + service.BufferMinutes;
        var slots = new List<AvailableSlotDto>();
        var current = schedule.StartTime;

        while (current.AddMinutes(slotDuration) <= schedule.EndTime)
        {
            var slotEnd = current.AddMinutes(slotDuration);

            var conflict = await HasSlotConflictAsync(
                request.StaffId, request.Date,
                current, slotEnd, null, ct);

            slots.Add(new AvailableSlotDto(current, slotEnd, !conflict));
            current = current.AddMinutes(slotDuration);
        }

        return slots.AsReadOnly();
    }

    // ── Payments ──────────────────────────────────────────────────────────
    public async Task<PaymentDto> RecordPaymentAsync(
        Guid bookingId, Guid adminId,
        RecordPaymentRequest request,
        CancellationToken ct = default)
    {
        var booking = await _db.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId, ct)
            ?? throw new NotFoundException(ResponseMessages.BookingNotFound);

        var payment = Payment.Create(
            bookingId, request.Amount,
            request.PaymentType, request.PaymentMethod,
            request.TransactionId);

        _db.Payments.Add(payment);

        if (request.PaymentType == PaymentType.Advance)
            booking.MarkAdvancePaid();

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Payment recorded for Booking {Code}: ₹{Amount} ({Type})",
            booking.BookingCode, request.Amount, request.PaymentType);

        return new PaymentDto(
            payment.Id, payment.Amount,
            payment.PaymentType, payment.PaymentMethod,
            payment.Status, payment.TransactionId,
            payment.PaidAt);
    }

    public async Task<IReadOnlyList<PaymentDto>> GetPaymentsAsync(
        Guid bookingId, CancellationToken ct = default)
    {
        var payments = await _db.Payments
            .AsNoTracking()
            .Where(p => p.BookingId == bookingId)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(ct);

        return payments.Select(p => new PaymentDto(
            p.Id, p.Amount, p.PaymentType,
            p.PaymentMethod, p.Status,
            p.TransactionId, p.PaidAt))
            .ToList().AsReadOnly();
    }

    // ── Consultation ──────────────────────────────────────────────────────
    public async Task<BookingDto> ScheduleConsultationAsync(
        Guid bookingId, Guid adminId,
        ScheduleConsultationRequest request,
        CancellationToken ct = default)
    {
        var booking = await _db.Bookings
            .FirstOrDefaultAsync(b =>
                b.Id == bookingId &&
                b.RequiresConsultation, ct)
            ?? throw new NotFoundException(ResponseMessages.BookingNotFound);

        booking.ScheduleConsultation(request.ScheduledAt);
        await _db.SaveChangesAsync(ct);

        return await GetByIdInternalAsync(bookingId, ct);
    }

    public async Task<BookingDto> CompleteConsultationAsync(
        Guid bookingId, Guid adminId,
        CancellationToken ct = default)
    {
        var booking = await _db.Bookings
            .FirstOrDefaultAsync(b =>
                b.Id == bookingId &&
                b.RequiresConsultation, ct)
            ?? throw new NotFoundException(ResponseMessages.BookingNotFound);

        booking.CompleteConsultation();
        await _db.SaveChangesAsync(ct);

        return await GetByIdInternalAsync(bookingId, ct);
    }

    // ── private ───────────────────────────────────────────────────────────
    private async Task<bool> HasSlotConflictAsync(
        Guid staffId, DateOnly date,
        TimeOnly startTime, TimeOnly endTime,
        Guid? excludeBookingId,
        CancellationToken ct)
    {
        var query = _db.Bookings
            .Where(b =>
                b.StaffId == staffId &&
                b.BookingDate == date &&
                b.Status != BookingStatus.Cancelled);

        if (excludeBookingId.HasValue)
            query = query.Where(b => b.Id != excludeBookingId);

        var existingBookings = await query
            .Include(b => b.Items)
            .ToListAsync(ct);

        foreach (var existing in existingBookings)
        {
            var existingDuration = existing.Items
                .Sum(i => i.DurationMinutes + i.BufferMinutes);
            var existingEnd = existing.BookingTime
                .AddMinutes(existingDuration);

            // Overlap check
            if (startTime < existingEnd &&
                endTime > existing.BookingTime)
                return true;
        }

        return false;
    }

    private async Task<Booking> GetBookingForStaffAsync(
        Guid bookingId, Guid staffUserId,
        CancellationToken ct)
    {
        var staff = await _db.StaffProfiles
            .FirstOrDefaultAsync(s => s.UserId == staffUserId, ct)
            ?? throw new NotFoundException(ResponseMessages.StaffNotFound);

        var booking = await _db.Bookings
            .FirstOrDefaultAsync(b =>
                b.Id == bookingId &&
                b.StaffId == staff.Id, ct)
            ?? throw new NotFoundException(ResponseMessages.BookingNotFound);

        return booking;
    }

    private async Task<string> GenerateBookingCodeAsync(CancellationToken ct)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"{BookingConstants.CodePrefix}-{today}-";
        var count = await _db.Bookings
            .CountAsync(b => b.BookingCode.StartsWith(prefix), ct);
        var code = $"{prefix}{(count + 1):D3}";
        return code;
    }

    private async Task<BookingDto> GetByIdInternalAsync(
        Guid bookingId, CancellationToken ct)
    {
        var b = await _db.Bookings
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Staff)
            .Include(x => x.Address)
            .Include(x => x.Items)
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.Id == bookingId, ct)
            ?? throw new NotFoundException(ResponseMessages.BookingNotFound);

        var totalDuration = b.Items.Sum(i => i.DurationMinutes + i.BufferMinutes);
        var estimatedEnd = b.BookingTime.AddMinutes(totalDuration);

        var addressLabel = b.Address is null ? null : b.Address.Label;
        var addressFull = b.Address is null ? null :
            $"{b.Address.AddressLine1}, {b.Address.City} - {b.Address.PinCode}";

        return new BookingDto(
            b.Id, b.BookingCode,
            b.CustomerId, b.Customer.FullName,
            b.StaffId, b.Staff.FullName, b.Staff.EmployeeCode,
            addressLabel, addressFull,
            b.BookingDate, b.BookingTime, estimatedEnd,
            b.BookingType, b.Status,
            b.TotalAmount, b.DiscountAmount, b.TravelCharge,
            b.FinalAmount, b.AdvanceAmount, b.AdvancePaid,
            b.CouponCode, b.LoyaltyPointsUsed, b.LoyaltyPointsEarned,
            b.Notes, b.RequiresConsultation,
            b.ConsultationScheduledAt, b.ConsultationDoneAt,
            b.CancellationReason, b.CancelledAt, b.CompletedAt,
            b.CreatedAt,
            b.Items.Select(i => new BookingItemDto(
                i.Id, i.ServiceId, i.ServiceName,
                i.Price, i.DurationMinutes,
                i.BufferMinutes, i.LoyaltyPoints))
                .ToList().AsReadOnly(),
            b.Payments.Select(p => new PaymentDto(
                p.Id, p.Amount, p.PaymentType,
                p.PaymentMethod, p.Status,
                p.TransactionId, p.PaidAt))
                .ToList().AsReadOnly());
    }

    private static BookingListDto MapToListDto(Booking b) =>
        new(b.Id, b.BookingCode,
            b.Customer.FullName, b.Staff.FullName,
            b.BookingDate, b.BookingTime,
            b.BookingType, b.Status,
            b.FinalAmount, b.Items.Count,
            b.CreatedAt);
}