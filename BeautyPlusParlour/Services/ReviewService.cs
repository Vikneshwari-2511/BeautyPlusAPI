using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Data;
using BeautyPlusParlour.Exceptions;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Review;
using BeautyPlusParlour.Models.Entities;
using BeautyPlusParlour.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BeautyPlusParlour.Services;

public sealed class ReviewService : IReviewService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(
        AppDbContext db,
        IAuditService audit,
        ILogger<ReviewService> logger)
    {
        _db = db;
        _audit = audit;
        _logger = logger;
    }

    // ── Create ────────────────────────────────────────────────────────────
    public async Task<ReviewDto> CreateAsync(
        Guid userId, CreateReviewRequest request,
        CancellationToken ct = default)
    {
        // Get customer
        var customer = await _db.CustomerProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive, ct)
            ?? throw new NotFoundException(ResponseMessages.CustomerNotFound);

        // Get booking — must be completed and belong to customer
        var booking = await _db.Bookings
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b =>
                b.Id == request.BookingId &&
                b.CustomerId == customer.Id, ct)
            ?? throw new NotFoundException(ResponseMessages.ReviewNotYourBooking);

        if (booking.Status != BookingStatus.Completed)
            throw new AppException(ResponseMessages.ReviewBookingNotCompleted);

        // Check already reviewed
        var alreadyReviewed = await _db.Reviews
            .AnyAsync(r => r.BookingId == request.BookingId, ct);

        if (alreadyReviewed)
            throw new AppException(ResponseMessages.ReviewAlreadyExists);

        // Use first service item for ServiceId
        var firstItem = booking.Items
            .OrderBy(i => i.Id)
            .First();

        var review = Review.Create(
            booking.Id, customer.Id,
            booking.StaffId, firstItem.ServiceId,
            request.ServiceRating, request.StaffRating,
            request.Comment);

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            userId, AuditAction.Created,
            "Review", review.Id.ToString(),
            null, null, ct: ct);

        _logger.LogInformation(
            "Review created for BookingId {BookingId} by CustomerId {CustomerId}",
            booking.Id, customer.Id);

        return await GetByIdInternalAsync(review.Id, ct);
    }

    // ── Update ────────────────────────────────────────────────────────────
    public async Task<ReviewDto> UpdateAsync(
        Guid reviewId, Guid userId,
        UpdateReviewRequest request,
        CancellationToken ct = default)
    {
        var customer = await _db.CustomerProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId, ct)
            ?? throw new NotFoundException(ResponseMessages.CustomerNotFound);

        var review = await _db.Reviews
            .FirstOrDefaultAsync(r =>
                r.Id == reviewId &&
                r.CustomerId == customer.Id, ct)
            ?? throw new NotFoundException(ResponseMessages.ReviewNotFound);

        if (!review.CanBeEdited())
            throw new AppException(ResponseMessages.ReviewEditExpired);

        review.Update(
            request.ServiceRating,
            request.StaffRating,
            request.Comment);

        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            userId, AuditAction.Updated,
            "Review", reviewId.ToString(),
            null, null, ct: ct);

        return await GetByIdInternalAsync(reviewId, ct);
    }

    // ── Hide (Admin) ──────────────────────────────────────────────────────
    public async Task<ReviewDto> HideAsync(
        Guid reviewId, Guid adminId,
        HideReviewRequest request,
        CancellationToken ct = default)
    {
        var review = await _db.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId, ct)
            ?? throw new NotFoundException(ResponseMessages.ReviewNotFound);

        review.Hide(adminId, request.Reason);
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            adminId, AuditAction.Updated,
            "Review", reviewId.ToString(),
            null, $"Hidden: {request.Reason}", ct: ct);

        _logger.LogInformation(
            "Review {ReviewId} hidden by AdminId {AdminId}",
            reviewId, adminId);

        return await GetByIdInternalAsync(reviewId, ct);
    }

    // ── Unhide (Admin) ────────────────────────────────────────────────────
    public async Task<ReviewDto> UnhideAsync(
        Guid reviewId, Guid adminId,
        CancellationToken ct = default)
    {
        var review = await _db.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId, ct)
            ?? throw new NotFoundException(ResponseMessages.ReviewNotFound);

        review.Unhide();
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            adminId, AuditAction.Updated,
            "Review", reviewId.ToString(),
            null, "Unhidden", ct: ct);

        return await GetByIdInternalAsync(reviewId, ct);
    }

    // ── Get by service (public) ───────────────────────────────────────────
    public async Task<IReadOnlyList<ReviewDto>> GetByServiceAsync(
        Guid serviceId, CancellationToken ct = default)
    {
        var reviews = await _db.Reviews
            .AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.Staff)
            .Include(r => r.Service)
            .Include(r => r.Booking)
            .Where(r => r.ServiceId == serviceId && r.IsVisible)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        return reviews.Select(MapToDto).ToList().AsReadOnly();
    }

    // ── Get by staff (public) ─────────────────────────────────────────────
    public async Task<IReadOnlyList<ReviewDto>> GetByStaffAsync(
        Guid staffId, CancellationToken ct = default)
    {
        var reviews = await _db.Reviews
            .AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.Staff)
            .Include(r => r.Service)
            .Include(r => r.Booking)
            .Where(r => r.StaffId == staffId && r.IsVisible)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        return reviews.Select(MapToDto).ToList().AsReadOnly();
    }

    // ── Get my reviews (customer) ─────────────────────────────────────────
    public async Task<IReadOnlyList<ReviewDto>> GetMyReviewsAsync(
        Guid userId, CancellationToken ct = default)
    {
        var customer = await _db.CustomerProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId, ct)
            ?? throw new NotFoundException(ResponseMessages.CustomerNotFound);

        var reviews = await _db.Reviews
            .AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.Staff)
            .Include(r => r.Service)
            .Include(r => r.Booking)
            .Where(r => r.CustomerId == customer.Id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        return reviews.Select(MapToDto).ToList().AsReadOnly();
    }

    // ── Get all (Admin) ───────────────────────────────────────────────────
    public async Task<IReadOnlyList<ReviewDto>> GetAllAsync(
        bool includeHidden, CancellationToken ct = default)
    {
        var query = _db.Reviews
            .AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.Staff)
            .Include(r => r.Service)
            .Include(r => r.Booking)
            .AsQueryable();

        if (!includeHidden)
            query = query.Where(r => r.IsVisible);

        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        return reviews.Select(MapToDto).ToList().AsReadOnly();
    }

    // ── Get by ID ─────────────────────────────────────────────────────────
    public async Task<ReviewDto> GetByIdAsync(
        Guid reviewId, CancellationToken ct = default)
    {
        return await GetByIdInternalAsync(reviewId, ct);
    }

    // ── Service summary ───────────────────────────────────────────────────
    public async Task<ReviewSummaryDto> GetServiceSummaryAsync(
        Guid serviceId, CancellationToken ct = default)
    {
        var service = await _db.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == serviceId, ct)
            ?? throw new NotFoundException(ResponseMessages.ServiceNotFound);

        var ratings = await _db.Reviews
            .AsNoTracking()
            .Where(r => r.ServiceId == serviceId && r.IsVisible)
            .Select(r => r.ServiceRating)
            .ToListAsync(ct);

        return BuildSummary(serviceId, service.Name, ratings);
    }

    // ── Staff summary ─────────────────────────────────────────────────────
    public async Task<ReviewSummaryDto> GetStaffSummaryAsync(
        Guid staffId, CancellationToken ct = default)
    {
        var staff = await _db.StaffProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == staffId, ct)
            ?? throw new NotFoundException(ResponseMessages.StaffNotFound);

        var ratings = await _db.Reviews
            .AsNoTracking()
            .Where(r => r.StaffId == staffId && r.IsVisible)
            .Select(r => r.StaffRating)
            .ToListAsync(ct);

        return BuildSummary(staffId, staff.FullName, ratings);
    }

    // ── private ───────────────────────────────────────────────────────────
    private async Task<ReviewDto> GetByIdInternalAsync(
        Guid reviewId, CancellationToken ct)
    {
        var review = await _db.Reviews
            .AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.Staff)
            .Include(r => r.Service)
            .Include(r => r.Booking)
            .FirstOrDefaultAsync(r => r.Id == reviewId, ct)
            ?? throw new NotFoundException(ResponseMessages.ReviewNotFound);

        return MapToDto(review);
    }

    private static ReviewSummaryDto BuildSummary(
        Guid entityId, string entityName,
        List<int> ratings)
    {
        if (ratings.Count == 0)
            return new ReviewSummaryDto(
                entityId, entityName, 0, 0, 0, 0, 0, 0, 0);

        return new ReviewSummaryDto(
            entityId, entityName,
            Math.Round(ratings.Average(), 1),
            ratings.Count,
            ratings.Count(r => r == 5),
            ratings.Count(r => r == 4),
            ratings.Count(r => r == 3),
            ratings.Count(r => r == 2),
            ratings.Count(r => r == 1));
    }

    private static ReviewDto MapToDto(Review r) =>
        new(r.Id, r.BookingId, r.Booking.BookingCode,
            r.CustomerId, r.Customer.FullName,
            r.StaffId, r.Staff.FullName,
            r.ServiceId, r.Service.Name,
            r.ServiceRating, r.StaffRating,
            r.Comment, r.IsVisible,
            r.CanBeEdited(),
            r.CreatedAt, r.UpdatedAt);
}