using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.Entities;

public sealed class StaffLeave
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid StaffId { get; private set; }
    public LeaveType LeaveType { get; private set; }
    public DateOnly LeaveFromDate { get; private set; }
    public DateOnly LeaveToDate { get; private set; }
    public int TotalDays { get; private set; }
    public string? Reason { get; private set; }
    public LeaveStatus Status { get; private set; } = LeaveStatus.Pending;
    public DateTime RequestedAt { get; private set; } = DateTime.UtcNow;
    public Guid? ReviewedBy { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public string? RejectionReason { get; private set; }

    // Navigation
    public StaffProfile Staff { get; private set; } = null!;

    private StaffLeave() { }

    public static StaffLeave Create(
        Guid staffId, LeaveType leaveType,
        DateOnly fromDate, DateOnly toDate,
        string? reason)
    {
        var totalDays = toDate.DayNumber - fromDate.DayNumber + 1;

        return new StaffLeave
        {
            StaffId = staffId,
            LeaveType = leaveType,
            LeaveFromDate = fromDate,
            LeaveToDate = toDate,
            TotalDays = totalDays,
            Reason = reason?.Trim()
        };
    }

    public void Approve(Guid reviewedBy)
    {
        Status = LeaveStatus.Approved;
        ReviewedBy = reviewedBy;
        ReviewedAt = DateTime.UtcNow;
    }

    public void Reject(Guid reviewedBy, string? reason)
    {
        Status = LeaveStatus.Rejected;
        ReviewedBy = reviewedBy;
        ReviewedAt = DateTime.UtcNow;
        RejectionReason = reason?.Trim();
    }

    public void Cancel() => Status = LeaveStatus.Rejected;

    public bool IsPending() => Status == LeaveStatus.Pending;
    public bool IsApproved() => Status == LeaveStatus.Approved;

    public bool OverlapsWith(DateOnly from, DateOnly to) =>
        LeaveFromDate <= to && LeaveToDate >= from
        && Status != LeaveStatus.Rejected;
}