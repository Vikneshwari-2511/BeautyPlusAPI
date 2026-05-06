using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.DTOs.Booking;

public sealed record CancelBookingRequest(string? Reason);

public sealed record RescheduleBookingRequest(
    Guid StaffId,
    DateOnly NewDate,
    TimeOnly NewTime
);

public sealed record RecordPaymentRequest(
    decimal Amount,
    PaymentType PaymentType,
    PaymentMethod PaymentMethod,
    string? TransactionId
);

public sealed record ScheduleConsultationRequest(
    DateTime ScheduledAt
);