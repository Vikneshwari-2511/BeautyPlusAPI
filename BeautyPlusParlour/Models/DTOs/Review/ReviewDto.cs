namespace BeautyPlusParlour.Models.DTOs.Review;

public sealed record CreateReviewRequest(
    Guid BookingId,
    int ServiceRating,
    int StaffRating,
    string? Comment
);

public sealed record UpdateReviewRequest(
    int ServiceRating,
    int StaffRating,
    string? Comment
);

public sealed record HideReviewRequest(
    string? Reason
);

public sealed record ReviewDto(
    Guid Id,
    Guid BookingId,
    string BookingCode,
    Guid CustomerId,
    string CustomerName,
    Guid StaffId,
    string StaffName,
    Guid ServiceId,
    string ServiceName,
    int ServiceRating,
    int StaffRating,
    string? Comment,
    bool IsVisible,
    bool CanEdit,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public sealed record ReviewSummaryDto(
    Guid EntityId,
    string EntityName,
    double AverageRating,
    int TotalReviews,
    int FiveStars,
    int FourStars,
    int ThreeStars,
    int TwoStars,
    int OneStar
);