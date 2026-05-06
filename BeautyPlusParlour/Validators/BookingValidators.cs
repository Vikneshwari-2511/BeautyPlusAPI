using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Models.DTOs.Booking;
using BeautyPlusParlour.Models.Enums;
using FluentValidation;

namespace BeautyPlusParlour.Validators;

public sealed class CreateBookingValidator
    : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingValidator()
    {
        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Staff selection is required.");

        RuleFor(x => x.BookingDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Booking date cannot be in the past.")
            .LessThanOrEqualTo(
                DateOnly.FromDateTime(
                    DateTime.Today.AddDays(BookingConstants.MaxAdvanceBookingDays)))
            .WithMessage($"Cannot book more than {BookingConstants.MaxAdvanceBookingDays} days in advance.");

        RuleFor(x => x.ServiceIds)
            .NotEmpty().WithMessage(ResponseMessages.BookingItemsRequired)
            .Must(s => s.Count <= BookingConstants.MaxItemsPerBooking)
            .WithMessage(ResponseMessages.BookingItemLimitExceeded);

        RuleFor(x => x.LoyaltyPointsToUse)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Loyalty points cannot be negative.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => x.Notes is not null);
    }
}

public sealed class RescheduleBookingValidator
    : AbstractValidator<RescheduleBookingRequest>
{
    public RescheduleBookingValidator()
    {
        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Staff selection is required.");

        RuleFor(x => x.NewDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Reschedule date cannot be in the past.");
    }
}

public sealed class RecordPaymentValidator
    : AbstractValidator<RecordPaymentRequest>
{
    public RecordPaymentValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than 0.");

        RuleFor(x => x.PaymentType)
            .IsInEnum().WithMessage("Invalid payment type.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Invalid payment method.");

        RuleFor(x => x.TransactionId)
            .MaximumLength(100)
            .When(x => x.TransactionId is not null);
    }
}