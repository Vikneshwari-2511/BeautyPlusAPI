namespace BeautyPlusParlour.Models.DTOs.SubCategory;

public sealed record CreateSubCategoryRequest(
    Guid CategoryId,
    string Name,
    int DisplayOrder = 0
);