using BeautyPlusParlour.Models.DTOs.Auth;
using FluentValidation;

namespace BeautyPlusParlour.Validators;

public sealed class ResendVerificationRequestValidator
    : AbstractValidator<ResendVerificationRequest>
{
    public ResendVerificationRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(256);
    }
}