using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Models.DTOs.Loyalty;
using FluentValidation;

namespace BeautyPlusParlour.Validators;

public sealed class AdjustPointsValidator
    : AbstractValidator<AdjustPointsRequest>
{
    public AdjustPointsValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer is required.");

        RuleFor(x => x.Points)
            .NotEqual(0).WithMessage("Points adjustment cannot be zero.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(200);
    }
}

public sealed class ValidateRedeemValidator
    : AbstractValidator<ValidateRedeemRequest>
{
    public ValidateRedeemValidator()
    {
        RuleFor(x => x.PointsToRedeem)
            .GreaterThanOrEqualTo(LoyaltyConstants.MinRedeemPoints)
            .WithMessage(ResponseMessages.MinRedeemPointsError);

        RuleFor(x => x.BookingTotal)
            .GreaterThan(0).WithMessage("Booking total must be greater than 0.");
    }
}