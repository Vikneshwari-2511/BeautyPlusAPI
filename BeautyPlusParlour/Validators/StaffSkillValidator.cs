using BeautyPlusParlour.Models.DTOs.Staff;
using FluentValidation;

namespace BeautyPlusParlour.Validators;

public sealed class AddSkillValidator : AbstractValidator<AddSkillRequest>
{
    public AddSkillValidator()
    {
        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("Service is required.");

        RuleFor(x => x.ProficiencyLevel)
            .IsInEnum().WithMessage("Invalid proficiency level.");
    }
}