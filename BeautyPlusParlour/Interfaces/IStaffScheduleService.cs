using BeautyPlusParlour.Models.DTOs.Staff;

namespace BeautyPlusParlour.Interfaces;

public interface IStaffScheduleService
{
    Task<IReadOnlyList<StaffScheduleDto>> GetAsync(Guid staffId, CancellationToken ct = default);
    Task<IReadOnlyList<StaffScheduleDto>> UpdateAsync(Guid staffId, IEnumerable<UpdateScheduleItemRequest> schedule, Guid updatedBy, CancellationToken ct = default);
}