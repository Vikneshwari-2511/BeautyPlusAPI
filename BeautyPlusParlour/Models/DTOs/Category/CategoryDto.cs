using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.DTOs.Category;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    ServiceType ServiceTypeDefault,
    string? ImageUrl,
    int DisplayOrder,
    bool IsActive,
    int ServiceCount,
    DateTime CreatedAt
);