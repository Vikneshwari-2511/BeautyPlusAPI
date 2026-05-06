namespace BeautyPlusParlour.Models.DTOs.SubCategory;

public sealed record SubCategoryDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Name,
    string Slug,
    int DisplayOrder,
    bool IsActive,
    int ServiceCount,
    DateTime CreatedAt
);