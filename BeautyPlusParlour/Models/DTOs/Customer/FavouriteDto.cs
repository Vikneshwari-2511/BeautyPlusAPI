using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.DTOs.Customer;

public sealed record FavouriteServiceDto(
    Guid Id,
    Guid ServiceId,
    string ServiceName,
    string CategoryName,
    string? SubCategoryName,
    ServiceType ServiceType,
    Gender Gender,
    decimal EffectivePrice,
    int DurationMinutes,
    int LoyaltyPoints,
    string? ImageUrl,
    DateTime AddedAt
);