using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Models.DTOs.Category;
using FluentValidation;

namespace BeautyPlusParlour.Validators;

public sealed class CreateCategoryValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.ServiceTypeDefault)
            .IsInEnum().WithMessage("Invalid service type.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters.")
            .When(x => x.ImageUrl is not null);

        RuleFor(x => x.DisplayOrder)
            .InclusiveBetween(0, ServiceConstants.MaxDisplayOrder)
            .WithMessage($"Display order must be between 0 and {ServiceConstants.MaxDisplayOrder}.");
    }
}

public sealed class UpdateCategoryValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100);

        RuleFor(x => x.ServiceTypeDefault)
            .IsInEnum().WithMessage("Invalid service type.");

        RuleFor(x => x.DisplayOrder)
            .InclusiveBetween(0, ServiceConstants.MaxDisplayOrder);
    }
}