using BeautyPlusParlour.Models.DTOs.Auth;
using FluentValidation;

namespace BeautyPlusParlour.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(150)
            .Matches(@"^[a-zA-Z\s]+$")
            .WithMessage("Full name must contain only letters and spaces.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessage("Passwords do not match.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^\+?[1-9]\d{9,14}$")
            .WithMessage("Enter a valid phone number (e.g. +919876543210).");
    }
}