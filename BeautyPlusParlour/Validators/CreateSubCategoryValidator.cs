using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Models.DTOs.SubCategory;
using FluentValidation;

namespace BeautyPlusParlour.Validators;

public sealed class CreateSubCategoryValidator
    : AbstractValidator<CreateSubCategoryRequest>
{
    public CreateSubCategoryValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Sub-category name is required.")
            .MaximumLength(100);

        RuleFor(x => x.DisplayOrder)
            .InclusiveBetween(0, ServiceConstants.MaxDisplayOrder);
    }
}

public sealed class UpdateSubCategoryValidator
    : AbstractValidator<UpdateSubCategoryRequest>
{
    public UpdateSubCategoryValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Sub-category name is required.")
            .MaximumLength(100);

        RuleFor(x => x.DisplayOrder)
            .InclusiveBetween(0, ServiceConstants.MaxDisplayOrder);
    }
}