using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Models.DTOs.Staff;
using BeautyPlusParlour.Models.Enums;
using FluentValidation;

namespace BeautyPlusParlour.Validators;

public sealed class RequestLeaveValidator
    : AbstractValidator<RequestLeaveRequest>
{
    public RequestLeaveValidator()
    {
        RuleFor(x => x.LeaveType)
            .IsInEnum().WithMessage("Invalid leave type.");

        RuleFor(x => x.LeaveFromDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage(ResponseMessages.LeaveDatePast);

        RuleFor(x => x.LeaveToDate)
            .GreaterThanOrEqualTo(x => x.LeaveFromDate)
            .WithMessage("Leave end date must be on or after start date.");

        RuleFor(x => x)
            .Must(x => (x.LeaveToDate.DayNumber - x.LeaveFromDate.DayNumber + 1)
                        <= StaffConstants.MaxLeaveRangeDays)
            .WithMessage(ResponseMessages.LeaveRangeExceeded)
            .When(x => x.LeaveType == LeaveType.Range);

        RuleFor(x => x)
            .Must(x => x.LeaveFromDate == x.LeaveToDate)
            .WithMessage("Single day leave must have same from and to date.")
            .When(x => x.LeaveType == LeaveType.SingleDay);

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .When(x => x.Reason is not null);
    }
}

public sealed class RejectLeaveValidator
    : AbstractValidator<RejectLeaveRequest>
{
    public RejectLeaveValidator()
    {
        RuleFor(x => x.RejectionReason)
            .MaximumLength(500)
            .When(x => x.RejectionReason is not null);
    }
}