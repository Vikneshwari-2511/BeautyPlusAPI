namespace BeautyPlusParlour.Models.DTOs.Booking;

public sealed record CreateBookingRequest(
    Guid StaffId,
    Guid? AddressId,
    DateOnly BookingDate,
    TimeOnly BookingTime,
    List<Guid> ServiceIds,
    string? CouponCode,
    int LoyaltyPointsToUse,
    string? Notes
);