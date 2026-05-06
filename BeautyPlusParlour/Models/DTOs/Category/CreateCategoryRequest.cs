using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.DTOs.Category;

public sealed record CreateCategoryRequest(
    string Name,
    string? Description,
    ServiceType ServiceTypeDefault,
    string? ImageUrl,
    int DisplayOrder = 0
);