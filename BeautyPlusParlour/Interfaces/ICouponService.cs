using BeautyPlusParlour.Models.DTOs.Coupon;

namespace BeautyPlusParlour.Interfaces;

public interface ICouponService
{
    Task<CouponDto> CreateAsync(CreateCouponRequest request, Guid adminId, CancellationToken ct = default);
    Task<CouponDto> UpdateAsync(Guid couponId, UpdateCouponRequest request, Guid adminId, CancellationToken ct = default);
    Task DeactivateAsync(Guid couponId, Guid adminId, CancellationToken ct = default);
    Task<CouponDto> GetByIdAsync(Guid couponId, CancellationToken ct = default);
    Task<IReadOnlyList<CouponDto>> GetAllAsync(bool includeInactive, CancellationToken ct = default);
    Task<ValidateCouponResponse> ValidateAsync(Guid userId, ValidateCouponRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<CouponUsageDto>> GetMyUsageAsync(Guid userId, CancellationToken ct = default);

    // Called internally by BookingService
    Task RecordUsageAsync(Guid couponId, Guid customerId, Guid bookingId, decimal discountApplied, CancellationToken ct = default);
}