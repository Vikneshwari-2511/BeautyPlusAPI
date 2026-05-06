namespace BeautyPlusParlour.Models.DTOs.SubCategory;

public sealed record UpdateSubCategoryRequest(
    string Name,
    Guid CategoryId,
    int DisplayOrder
);