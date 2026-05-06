using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.DTOs.Service;

public sealed record CreateServiceRequest(
    Guid CategoryId,
    Guid? SubCategoryId,
    string Name,
    string? Description,
    ServiceType ServiceTypeActual,
    Gender Gender,
    decimal BasePrice,
    decimal? DiscountedPrice,
    int DurationMinutes,
    int BufferMinutes,
    int? LoyaltyPointsOverride,
    string? ImageUrl,
    int DisplayOrder,
    bool IsFeatured,
    bool IsPopular,
    bool RequiresConsultation,
    bool IsTaxInclusive,
    decimal? TaxPercent,
    CreateOnSiteDetailRequest? OnSiteDetail
);

public sealed record CreateOnSiteDetailRequest(
    decimal TravelCharge,
    int AdvancePercent,
    int MinBookingDays,
    string? SpecialNotes
);