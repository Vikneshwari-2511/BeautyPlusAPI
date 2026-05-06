using BeautyPlusParlour.Models.DTOs.Staff;
using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Interfaces;

public interface IStaffLeaveService
{
    Task<StaffLeaveDto> RequestLeaveAsync(Guid userId, RequestLeaveRequest request, CancellationToken ct = default);
    Task<StaffLeaveDto> ApproveAsync(Guid leaveId, Guid adminId, CancellationToken ct = default);
    Task<StaffLeaveDto> RejectAsync(Guid leaveId, Guid adminId, RejectLeaveRequest request, CancellationToken ct = default);
    Task CancelAsync(Guid leaveId, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<StaffLeaveDto>> GetMyLeavesAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<StaffLeaveDto>> GetPendingAsync(CancellationToken ct = default);
    Task<IReadOnlyList<StaffLeaveDto>> GetAllAsync(Guid? staffId, LeaveStatus? status, CancellationToken ct = default);
}