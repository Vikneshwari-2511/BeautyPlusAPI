using BeautyPlusParlour.Models.DTOs.Review;
using FluentValidation;

namespace BeautyPlusParlour.Validators;

public sealed class CreateReviewValidator
    : AbstractValidator<CreateReviewRequest>
{
    public CreateReviewValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty().WithMessage("Booking is required.");

        RuleFor(x => x.ServiceRating)
            .InclusiveBetween(1, 5)
            .WithMessage("Service rating must be between 1 and 5.");

        RuleFor(x => x.StaffRating)
            .InclusiveBetween(1, 5)
            .WithMessage("Staff rating must be between 1 and 5.");

        RuleFor(x => x.Comment)
            .MaximumLength(1000)
            .WithMessage("Comment must not exceed 1000 characters.")
            .When(x => x.Comment is not null);
    }
}

public sealed class UpdateReviewValidator
    : AbstractValidator<UpdateReviewRequest>
{
    public UpdateReviewValidator()
    {
        RuleFor(x => x.ServiceRating)
            .InclusiveBetween(1, 5)
            .WithMessage("Service rating must be between 1 and 5.");

        RuleFor(x => x.StaffRating)
            .InclusiveBetween(1, 5)
            .WithMessage("Staff rating must be between 1 and 5.");

        RuleFor(x => x.Comment)
            .MaximumLength(1000)
            .When(x => x.Comment is not null);
    }
}