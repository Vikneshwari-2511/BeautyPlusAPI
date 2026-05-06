using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.DTOs.Staff;

public sealed record RequestLeaveRequest(
    LeaveType LeaveType,
    DateOnly LeaveFromDate,
    DateOnly LeaveToDate,
    string? Reason
);

public sealed record RejectLeaveRequest(
    string? RejectionReason
);

public sealed record StaffLeaveDto(
    Guid Id,
    Guid StaffId,
    string StaffName,
    string EmployeeCode,
    LeaveType LeaveType,
    DateOnly LeaveFromDate,
    DateOnly LeaveToDate,
    int TotalDays,
    string? Reason,
    LeaveStatus Status,
    DateTime RequestedAt,
    string? ReviewedByName,
    DateTime? ReviewedAt,
    string? RejectionReason
);