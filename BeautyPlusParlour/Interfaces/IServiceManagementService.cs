using BeautyPlusParlour.Models.DTOs.Common;
using BeautyPlusParlour.Models.DTOs.Service;

namespace BeautyPlusParlour.Interfaces;

public interface IServiceManagementService
{
    Task<ServiceDto> CreateAsync(CreateServiceRequest request, Guid adminId, CancellationToken ct = default);
    Task<ServiceDto> UpdateAsync(Guid id, UpdateServiceRequest request, Guid adminId, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid adminId, CancellationToken ct = default);
    Task ToggleActiveAsync(Guid id, Guid adminId, CancellationToken ct = default);
    Task<ServiceDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResponse<ServiceListDto>> GetAllAsync(ServiceFilterRequest filter, CancellationToken ct = default);
    Task<IReadOnlyList<ServiceListDto>> GetFeaturedAsync(CancellationToken ct = default);
}