using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Data;
using BeautyPlusParlour.Exceptions;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Coupon;
using BeautyPlusParlour.Models.Entities;
using BeautyPlusParlour.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BeautyPlusParlour.Services;

public sealed class CouponService : ICouponService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly ILogger<CouponService> _logger;

    public CouponService(
        AppDbContext db,
        IAuditService audit,
        ILogger<CouponService> logger)
    {
        _db = db;
        _audit = audit;
        _logger = logger;
    }

    // ── Create ────────────────────────────────────────────────────────────
    public async Task<CouponDto> CreateAsync(
        CreateCouponRequest request, Guid adminId,
        CancellationToken ct = default)
    {
        var codeExists = await _db.Coupons
            .AnyAsync(c => c.Code == request.Code.ToUpperInvariant(), ct);

        if (codeExists)
            throw new AppException(ResponseMessages.CouponCodeExists);

        var coupon = Coupon.Create(
            request.Code, request.Description,
            request.CouponType, request.Value,
            request.MinOrderAmount, request.MaxDiscount,
            request.UsageLimit, request.PerUserLimit,
            request.ValidFrom, request.ValidTo, adminId);

        _db.Coupons.Add(coupon);
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            adminId, AuditAction.Created,
            "Coupon", coupon.Id.ToString(),
            null, JsonSerializer.Serialize(new
            {
                coupon.Code,
                coupon.CouponType,
                coupon.Value
            }), ct: ct);

        _logger.LogInformation(
            "Coupon created: {Code} ({Type}: {Value}) by AdminId {AdminId}",
            coupon.Code, coupon.CouponType, coupon.Value, adminId);

        return MapToDto(coupon);
    }

    // ── Update ────────────────────────────────────────────────────────────
    public async Task<CouponDto> UpdateAsync(
        Guid couponId, UpdateCouponRequest request,
        Guid adminId, CancellationToken ct = default)
    {
        var coupon = await _db.Coupons
            .FirstOrDefaultAsync(c => c.Id == couponId, ct)
            ?? throw new NotFoundException(ResponseMessages.CouponNotFound);

        var oldValues = JsonSerializer.Serialize(new
        {
            coupon.Value,
            coupon.ValidFrom,
            coupon.ValidTo
        });

        coupon.Update(
            request.Description, request.Value,
            request.MinOrderAmount, request.MaxDiscount,
            request.UsageLimit, request.PerUserLimit,
            request.ValidFrom, request.ValidTo);

        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            adminId, AuditAction.Updated,
            "Coupon", couponId.ToString(),
            oldValues,
            JsonSerializer.Serialize(new { request.Value }),
            ct: ct);

        return MapToDto(coupon);
    }

    // ── Deactivate ────────────────────────────────────────────────────────
    public async Task DeactivateAsync(
        Guid couponId, Guid adminId,
        CancellationToken ct = default)
    {
        var coupon = await _db.Coupons
            .FirstOrDefaultAsync(c => c.Id == couponId, ct)
            ?? throw new NotFoundException(ResponseMessages.CouponNotFound);

        coupon.Deactivate();
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            adminId, AuditAction.Deleted,
            "Coupon", couponId.ToString(), ct: ct);
    }

    // ── Get by ID ─────────────────────────────────────────────────────────
    public async Task<CouponDto> GetByIdAsync(
        Guid couponId, CancellationToken ct = default)
    {
        var coupon = await _db.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == couponId, ct)
            ?? throw new NotFoundException(ResponseMessages.CouponNotFound);

        return MapToDto(coupon);
    }

    // ── Get all ───────────────────────────────────────────────────────────
    public async Task<IReadOnlyList<CouponDto>> GetAllAsync(
        bool includeInactive, CancellationToken ct = default)
    {
        var query = _db.Coupons.AsNoTracking();

        if (!includeInactive)
            query = query.Where(c => c.IsActive);

        var coupons = await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);

        return coupons.Select(MapToDto).ToList().AsReadOnly();
    }

    // ── Validate ──────────────────────────────────────────────────────────
    public async Task<ValidateCouponResponse> ValidateAsync(
        Guid userId, ValidateCouponRequest request,
        CancellationToken ct = default)
    {
        var customer = await _db.CustomerProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId, ct)
            ?? throw new NotFoundException(ResponseMessages.CustomerNotFound);

        var coupon = await _db.Coupons
            .FirstOrDefaultAsync(c =>
                c.Code == request.Code.ToUpperInvariant() &&
                c.IsActive, ct);

        if (coupon is null)
            return Fail(ResponseMessages.CouponInvalid, request.OrderTotal);

        if (coupon.IsExpired())
            return Fail(ResponseMessages.CouponExpired, request.OrderTotal);

        if (!coupon.IsValid())
            return Fail(ResponseMessages.CouponNotActive, request.OrderTotal);

        if (coupon.IsLimitReached())
            return Fail(ResponseMessages.CouponUsageLimitReached, request.OrderTotal);

        if (request.OrderTotal < coupon.MinOrderAmount)
            return Fail(
                $"{ResponseMessages.CouponMinOrderNotMet} " +
                $"(Min: ₹{coupon.MinOrderAmount:F2})",
                request.OrderTotal);

        // Check per-user limit
        var userUsageCount = await _db.CouponUsages
            .CountAsync(u =>
                u.CouponId == coupon.Id &&
                u.CustomerId == customer.Id, ct);

        if (userUsageCount >= coupon.PerUserLimit)
            return Fail(ResponseMessages.CouponPerUserLimitReached, request.OrderTotal);

        var discount = coupon.CalculateDiscount(request.OrderTotal);
        var finalAmount = request.OrderTotal - discount;

        return new ValidateCouponResponse(
            true, ResponseMessages.CouponValid,
            coupon.Code, discount, finalAmount);
    }

    // ── Get my usage ──────────────────────────────────────────────────────
    public async Task<IReadOnlyList<CouponUsageDto>> GetMyUsageAsync(
        Guid userId, CancellationToken ct = default)
    {
        var customer = await _db.CustomerProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId, ct)
            ?? throw new NotFoundException(ResponseMessages.CustomerNotFound);

        var usages = await _db.CouponUsages
            .AsNoTracking()
            .Include(u => u.Coupon)
            .Include(u => u.Booking)
            .Where(u => u.CustomerId == customer.Id)
            .OrderByDescending(u => u.UsedAt)
            .ToListAsync(ct);

        return usages.Select(u => new CouponUsageDto(
            u.Id, u.Coupon.Code,
            u.Booking.BookingCode,
            u.DiscountApplied, u.UsedAt))
            .ToList().AsReadOnly();
    }

    // ── Record usage (called by BookingService) ────────────────────────────
    public async Task RecordUsageAsync(
        Guid couponId, Guid customerId,
        Guid bookingId, decimal discountApplied,
        CancellationToken ct = default)
    {
        var coupon = await _db.Coupons
            .FirstOrDefaultAsync(c => c.Id == couponId, ct)
            ?? throw new NotFoundException(ResponseMessages.CouponNotFound);

        coupon.IncrementUsage();

        var usage = CouponUsage.Create(
            couponId, customerId, bookingId, discountApplied);

        _db.CouponUsages.Add(usage);
        await _db.SaveChangesAsync(ct);
    }

    // ── private ───────────────────────────────────────────────────────────
    private static ValidateCouponResponse Fail(
        string message, decimal orderTotal) =>
        new(false, message, null, 0m, orderTotal);

    private static CouponDto MapToDto(Coupon c) =>
        new(c.Id, c.Code, c.Description,
            c.CouponType, c.Value,
            c.MinOrderAmount, c.MaxDiscount,
            c.UsageLimit, c.PerUserLimit,
            c.UsedCount, c.ValidFrom, c.ValidTo,
            c.IsActive, c.IsExpired(),
            c.CreatedAt);
}