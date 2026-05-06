using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.DTOs.Coupon;

public sealed record CreateCouponRequest(
    string Code,
    string Description,
    CouponType CouponType,
    decimal Value,
    decimal MinOrderAmount,
    decimal? MaxDiscount,
    int? UsageLimit,
    int PerUserLimit,
    DateTime ValidFrom,
    DateTime ValidTo
);

public sealed record UpdateCouponRequest(
    string Description,
    decimal Value,
    decimal MinOrderAmount,
    decimal? MaxDiscount,
    int? UsageLimit,
    int PerUserLimit,
    DateTime ValidFrom,
    DateTime ValidTo
);

public sealed record CouponDto(
    Guid Id,
    string Code,
    string Description,
    CouponType CouponType,
    decimal Value,
    decimal MinOrderAmount,
    decimal? MaxDiscount,
    int? UsageLimit,
    int PerUserLimit,
    int UsedCount,
    DateTime ValidFrom,
    DateTime ValidTo,
    bool IsActive,
    bool IsExpired,
    DateTime CreatedAt
);

public sealed record ValidateCouponRequest(
    string Code,
    decimal OrderTotal
);

public sealed record ValidateCouponResponse(
    bool IsValid,
    string Message,
    string? Code,
    decimal DiscountAmount,
    decimal FinalAmount
);

public sealed record CouponUsageDto(
    Guid Id,
    string CouponCode,
    string BookingCode,
    decimal DiscountApplied,
    DateTime UsedAt
);