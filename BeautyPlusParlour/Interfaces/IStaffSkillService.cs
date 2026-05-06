using BeautyPlusParlour.Models.DTOs.Staff;

namespace BeautyPlusParlour.Interfaces;

public interface IStaffSkillService
{
    Task<StaffSkillDto> AddAsync(Guid staffId, AddSkillRequest request, Guid adminId, CancellationToken ct = default);
    Task RemoveAsync(Guid staffId, Guid skillId, Guid adminId, CancellationToken ct = default);
    Task<IReadOnlyList<StaffSkillDto>> GetByStaffIdAsync(Guid staffId, CancellationToken ct = default);
}