using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Models.DTOs.Service;
using BeautyPlusParlour.Models.Enums;
using FluentValidation;

namespace BeautyPlusParlour.Validators;

public sealed class CreateServiceValidator : AbstractValidator<CreateServiceRequest>
{
    public CreateServiceValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Service name is required.")
            .MaximumLength(150);

        RuleFor(x => x.ServiceTypeActual)
            .IsInEnum().WithMessage("Invalid service type.");

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Invalid gender.");

        RuleFor(x => x.BasePrice)
            .GreaterThan(0).WithMessage("Base price must be greater than 0.");

        RuleFor(x => x.DiscountedPrice)
            .GreaterThanOrEqualTo(0)
                .WithMessage("Discounted price must be >= 0.")
            .LessThan(x => x.BasePrice)
                .WithMessage("Discounted price must be less than base price.")
            .When(x => x.DiscountedPrice.HasValue);

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(
                ServiceConstants.MinDurationMinutes,
                ServiceConstants.MaxDurationMinutes)
            .WithMessage($"Duration must be between " +
                $"{ServiceConstants.MinDurationMinutes} and " +
                $"{ServiceConstants.MaxDurationMinutes} minutes.");

        RuleFor(x => x.BufferMinutes)
            .InclusiveBetween(0, ServiceConstants.MaxBufferMinutes)
            .WithMessage($"Buffer time must be between 0 and " +
                $"{ServiceConstants.MaxBufferMinutes} minutes.");

        RuleFor(x => x.LoyaltyPointsOverride)
            .GreaterThanOrEqualTo(0)
                .WithMessage("Loyalty points must be >= 0.")
            .When(x => x.LoyaltyPointsOverride.HasValue);

        RuleFor(x => x.TaxPercent)
            .InclusiveBetween(0, 100)
                .WithMessage("Tax percent must be between 0 and 100.")
            .When(x => x.TaxPercent.HasValue);

        RuleFor(x => x.DisplayOrder)
            .InclusiveBetween(0, ServiceConstants.MaxDisplayOrder);

        // OnSiteDetail required when type is OnSite or Both
        RuleFor(x => x.OnSiteDetail)
            .NotNull()
                .WithMessage(ResponseMessages.OnSiteDetailRequired)
            .When(x => x.ServiceTypeActual == ServiceType.OnSite
                     || x.ServiceTypeActual == ServiceType.Both);

        RuleFor(x => x.OnSiteDetail!.TravelCharge)
            .GreaterThanOrEqualTo(0)
                .WithMessage("Travel charge must be >= 0.")
            .When(x => x.OnSiteDetail is not null);

        RuleFor(x => x.OnSiteDetail!.AdvancePercent)
            .InclusiveBetween(
                ServiceConstants.AdvancePercentMin,
                ServiceConstants.AdvancePercentMax)
            .When(x => x.OnSiteDetail is not null);

        RuleFor(x => x.OnSiteDetail!.MinBookingDays)
            .InclusiveBetween(
                ServiceConstants.MinBookingDaysMin,
                ServiceConstants.MinBookingDaysMax)
            .When(x => x.OnSiteDetail is not null);
    }
}

public sealed class UpdateServiceValidator : AbstractValidator<UpdateServiceRequest>
{
    public UpdateServiceValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Service name is required.")
            .MaximumLength(150);

        RuleFor(x => x.ServiceTypeActual)
            .IsInEnum().WithMessage("Invalid service type.");

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Invalid gender.");

        RuleFor(x => x.BasePrice)
            .GreaterThan(0).WithMessage("Base price must be greater than 0.");

        RuleFor(x => x.DiscountedPrice)
            .GreaterThanOrEqualTo(0)
            .LessThan(x => x.BasePrice)
                .WithMessage("Discounted price must be less than base price.")
            .When(x => x.DiscountedPrice.HasValue);

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(
                ServiceConstants.MinDurationMinutes,
                ServiceConstants.MaxDurationMinutes);

        RuleFor(x => x.BufferMinutes)
            .InclusiveBetween(0, ServiceConstants.MaxBufferMinutes);

        RuleFor(x => x.OnSiteDetail)
            .NotNull()
                .WithMessage(ResponseMessages.OnSiteDetailRequired)
            .When(x => x.ServiceTypeActual == ServiceType.OnSite
                     || x.ServiceTypeActual == ServiceType.Both);
    }
}