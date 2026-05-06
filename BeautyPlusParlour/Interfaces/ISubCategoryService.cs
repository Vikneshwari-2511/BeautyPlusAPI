using BeautyPlusParlour.Models.DTOs.SubCategory;

namespace BeautyPlusParlour.Interfaces;

public interface ISubCategoryService
{
    Task<SubCategoryDto> CreateAsync(CreateSubCategoryRequest request, Guid adminId, CancellationToken ct = default);
    Task<SubCategoryDto> UpdateAsync(Guid id, UpdateSubCategoryRequest request, Guid adminId, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid adminId, CancellationToken ct = default);
    Task<SubCategoryDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<SubCategoryDto>> GetAllAsync(CancellationToken ct = default);
}