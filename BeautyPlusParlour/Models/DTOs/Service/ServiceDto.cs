using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.DTOs.Service;

public sealed record ServiceDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    Guid? SubCategoryId,
    string? SubCategoryName,
    string Name,
    string Slug,
    string? Description,
    ServiceType ServiceTypeActual,
    Gender Gender,
    decimal BasePrice,
    decimal? DiscountedPrice,
    decimal EffectivePrice,
    int DurationMinutes,
    int BufferMinutes,
    int TotalSlotMinutes,
    int LoyaltyPoints,
    string? ImageUrl,
    int DisplayOrder,
    bool IsActive,
    bool IsFeatured,
    bool IsPopular,
    bool RequiresConsultation,
    bool IsTaxInclusive,
    decimal? TaxPercent,
    OnSiteDetailDto? OnSiteDetail,
    DateTime CreatedAt
);

public sealed record ServiceListDto(
    Guid Id,
    string Name,
    string Slug,
    string CategoryName,
    string? SubCategoryName,
    ServiceType ServiceTypeActual,
    Gender Gender,
    decimal EffectivePrice,
    int DurationMinutes,
    int LoyaltyPoints,
    string? ImageUrl,
    bool IsFeatured,
    bool IsPopular
);

public sealed record OnSiteDetailDto(
    decimal TravelCharge,
    int AdvancePercent,
    int MinBookingDays,
    string? SpecialNotes
);