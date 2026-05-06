using BeautyPlusParlour.Models.DTOs.Coupon;
using BeautyPlusParlour.Models.Enums;
using FluentValidation;

namespace BeautyPlusParlour.Validators;

public sealed class CreateCouponValidator
    : AbstractValidator<CreateCouponRequest>
{
    public CreateCouponValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Coupon code is required.")
            .MaximumLength(50)
            .Matches(@"^[A-Z0-9_]+$").WithMessage(
                "Code must contain only uppercase letters, numbers, or underscores.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(200);

        RuleFor(x => x.CouponType)
            .IsInEnum().WithMessage("Invalid coupon type.");

        RuleFor(x => x.Value)
            .GreaterThan(0).WithMessage("Coupon value must be greater than 0.");

        RuleFor(x => x.Value)
            .LessThanOrEqualTo(100)
            .WithMessage("Percentage discount cannot exceed 100%.")
            .When(x => x.CouponType == CouponType.Percentage);

        RuleFor(x => x.MinOrderAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Minimum order amount cannot be negative.");

        RuleFor(x => x.MaxDiscount)
            .GreaterThan(0)
            .WithMessage("Max discount must be greater than 0.")
            .When(x => x.MaxDiscount.HasValue);

        RuleFor(x => x.UsageLimit)
            .GreaterThan(0)
            .WithMessage("Usage limit must be greater than 0.")
            .When(x => x.UsageLimit.HasValue);

        RuleFor(x => x.PerUserLimit)
            .GreaterThan(0).WithMessage("Per user limit must be at least 1.");

        RuleFor(x => x.ValidFrom)
            .NotEmpty().WithMessage("Valid from date is required.");

        RuleFor(x => x.ValidTo)
            .GreaterThan(x => x.ValidFrom)
            .WithMessage("Valid to date must be after valid from date.");
    }
}

public sealed class UpdateCouponValidator
    : AbstractValidator<UpdateCouponRequest>
{
    public UpdateCouponValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().MaximumLength(200);

        RuleFor(x => x.Value)
            .GreaterThan(0);

        RuleFor(x => x.MinOrderAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.PerUserLimit)
            .GreaterThan(0);

        RuleFor(x => x.ValidTo)
            .GreaterThan(x => x.ValidFrom)
            .WithMessage("Valid to must be after valid from.");
    }
}

public sealed class ValidateCouponValidator
    : AbstractValidator<ValidateCouponRequest>
{
    public ValidateCouponValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Coupon code is required.");

        RuleFor(x => x.OrderTotal)
            .GreaterThan(0).WithMessage("Order total must be greater than 0.");
    }
}