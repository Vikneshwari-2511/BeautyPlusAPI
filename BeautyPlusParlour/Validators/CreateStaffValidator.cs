using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Models.DTOs.Staff;
using FluentValidation;

namespace BeautyPlusParlour.Validators;

public sealed class CreateStaffValidator
    : AbstractValidator<CreateStaffRequest>
{
    public CreateStaffValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User account is required.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(150);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?[1-9]\d{9,14}$")
            .WithMessage("Enter a valid phone number.");

        RuleFor(x => x.Designation)
            .IsInEnum().WithMessage("Invalid designation.");

        RuleFor(x => x.ExperienceYears)
            .InclusiveBetween(0, StaffConstants.MaxExperienceYears)
            .WithMessage($"Experience must be between 0 and {StaffConstants.MaxExperienceYears} years.");

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Invalid gender.");

        RuleFor(x => x.JoinedAt)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Joining date cannot be in the future.");

        RuleFor(x => x.Bio)
            .MaximumLength(1000)
            .When(x => x.Bio is not null);
    }
}

public sealed class UpdateStaffValidator
    : AbstractValidator<UpdateStaffRequest>
{
    public UpdateStaffValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(150);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^\+?[1-9]\d{9,14}$")
            .WithMessage("Enter a valid phone number.");

        RuleFor(x => x.Designation)
            .IsInEnum().WithMessage("Invalid designation.");

        RuleFor(x => x.ExperienceYears)
            .InclusiveBetween(0, StaffConstants.MaxExperienceYears);

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Invalid gender.");

        RuleFor(x => x.JoinedAt)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Joining date cannot be in the future.");
    }
}

public sealed class UpdateOwnProfileValidator
    : AbstractValidator<UpdateOwnProfileRequest>
{
    public UpdateOwnProfileValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^\+?[1-9]\d{9,14}$")
            .WithMessage("Enter a valid phone number.");

        RuleFor(x => x.Bio)
            .MaximumLength(1000)
            .When(x => x.Bio is not null);

        RuleFor(x => x.ProfileImageUrl)
            .MaximumLength(500)
            .When(x => x.ProfileImageUrl is not null);
    }
}