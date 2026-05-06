using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.DTOs.Category;

public sealed record UpdateCategoryRequest(
    string Name,
    string? Description,
    ServiceType ServiceTypeDefault,
    string? ImageUrl,
    int DisplayOrder
);