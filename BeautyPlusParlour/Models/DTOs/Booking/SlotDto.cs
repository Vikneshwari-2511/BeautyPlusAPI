namespace BeautyPlusParlour.Models.DTOs.Booking;

public sealed record AvailableSlotDto(
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsAvailable
);

public sealed record AvailableSlotsRequest(
    Guid ServiceId,
    Guid StaffId,
    DateOnly Date
);