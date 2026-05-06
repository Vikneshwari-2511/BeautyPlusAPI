using BeautyPlusParlour.Models.DTOs.Staff;

namespace BeautyPlusParlour.Interfaces;

public interface IStaffService
{
    Task<StaffDto> CreateAsync(CreateStaffRequest request, Guid adminId, CancellationToken ct = default);
    Task<StaffDto> UpdateAsync(Guid staffId, UpdateStaffRequest request, Guid adminId, CancellationToken ct = default);
    Task<StaffDto> UpdateOwnProfileAsync(Guid userId, UpdateOwnProfileRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid staffId, Guid adminId, CancellationToken ct = default);
    Task<StaffDto> GetByIdAsync(Guid staffId, CancellationToken ct = default);
    Task<StaffDto> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<StaffListDto>> GetAllAsync(bool includeInactive, CancellationToken ct = default);
    Task<IReadOnlyList<StaffAvailabilityDto>> GetAvailableForServiceAsync(Guid serviceId, DateOnly date, TimeOnly time, CancellationToken ct = default);
}