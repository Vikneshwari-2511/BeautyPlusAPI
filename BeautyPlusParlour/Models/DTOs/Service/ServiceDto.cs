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
    Guid Id,               // 1
    string Name,             // 2
    string Slug,             // 3
    string CategoryName,     // 4
    string? SubCategoryName,  // 5
    ServiceType ServiceTypeActual,// 6
    Gender Gender,           // 7
    decimal BasePrice,        // 8
    decimal? DiscountedPrice,  // 9
    decimal EffectivePrice,   // 10
    int DurationMinutes,  // 11
    int BufferMinutes,    // 12
    int LoyaltyPoints,    // 13
    string? ImageUrl,         // 14  ← position matters
    bool IsActive,         // 15
    bool IsFeatured,       // 16
    bool IsPopular         // 17
);

public sealed record OnSiteDetailDto(
    decimal TravelCharge,
    int AdvancePercent,
    int MinBookingDays,
    string? SpecialNotes
);