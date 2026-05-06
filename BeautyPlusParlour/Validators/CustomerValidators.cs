using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Models.DTOs.Customer;
using FluentValidation;

namespace BeautyPlusParlour.Validators;

public sealed class UpdateProfileValidator
    : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(150);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?[1-9]\d{9,14}$")
            .WithMessage("Enter a valid phone number.");

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Invalid gender.");

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Date of birth must be in the past.")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.ProfileImageUrl)
            .MaximumLength(500)
            .When(x => x.ProfileImageUrl is not null);
    }
}

public sealed class CreateAddressValidator
    : AbstractValidator<CreateAddressRequest>
{
    public CreateAddressValidator()
    {
        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("Label is required (e.g. Home, Work).")
            .MaximumLength(50);

        RuleFor(x => x.AddressLine1)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(250);

        RuleFor(x => x.AddressLine2)
            .MaximumLength(250)
            .When(x => x.AddressLine2 is not null);

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100);

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required.")
            .MaximumLength(100);

        RuleFor(x => x.PinCode)
            .NotEmpty().WithMessage("Pin code is required.")
            .Length(
                CustomerConstants.MinPinCodeLength,
                CustomerConstants.MaxPinCodeLength)
            .WithMessage($"Pin code must be between " +
                $"{CustomerConstants.MinPinCodeLength} and " +
                $"{CustomerConstants.MaxPinCodeLength} characters.")
            .Matches(@"^\d+$")
            .WithMessage("Pin code must contain only digits.");

        RuleFor(x => x.Landmark)
            .MaximumLength(200)
            .When(x => x.Landmark is not null);
    }
}

public sealed class UpdateAddressValidator
    : AbstractValidator<UpdateAddressRequest>
{
    public UpdateAddressValidator()
    {
        RuleFor(x => x.Label)
            .NotEmpty().MaximumLength(50);

        RuleFor(x => x.AddressLine1)
            .NotEmpty().MaximumLength(250);

        RuleFor(x => x.City)
            .NotEmpty().MaximumLength(100);

        RuleFor(x => x.State)
            .NotEmpty().MaximumLength(100);

        RuleFor(x => x.PinCode)
            .NotEmpty()
            .Length(
                CustomerConstants.MinPinCodeLength,
                CustomerConstants.MaxPinCodeLength)
            .Matches(@"^\d+$")
            .WithMessage("Pin code must contain only digits.");
    }
}