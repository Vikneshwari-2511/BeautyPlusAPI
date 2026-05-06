using BeautyPlusParlour.Models.DTOs.Category;
using BeautyPlusParlour.Models.DTOs.Common;
using BeautyPlusParlour.Models.DTOs.SubCategory;

namespace BeautyPlusParlour.Interfaces;

public interface ICategoryService
{
    Task<CategoryDto>               CreateAsync(CreateCategoryRequest request, Guid adminId, CancellationToken ct = default);
    Task<CategoryDto>               UpdateAsync(Guid id, UpdateCategoryRequest request, Guid adminId, CancellationToken ct = default);
    Task                            DeleteAsync(Guid id, Guid adminId, CancellationToken ct = default);
    Task<CategoryDto>               GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(bool includeInactive, CancellationToken ct = default);
    Task<IReadOnlyList<SubCategoryDto>> GetSubCategoriesAsync(Guid categoryId, CancellationToken ct = default);
}