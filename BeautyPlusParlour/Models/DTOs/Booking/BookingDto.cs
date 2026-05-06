using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.DTOs.Booking;

public sealed record BookingDto(
    Guid Id,
    string BookingCode,
    Guid CustomerId,
    string CustomerName,
    Guid StaffId,
    string StaffName,
    string StaffEmployeeCode,
    string? AddressLabel,
    string? AddressFull,
    DateOnly BookingDate,
    TimeOnly BookingTime,
    TimeOnly EstimatedEndTime,
    BookingType BookingType,
    BookingStatus Status,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal TravelCharge,
    decimal FinalAmount,
    decimal AdvanceAmount,
    bool AdvancePaid,
    string? CouponCode,
    int LoyaltyPointsUsed,
    int LoyaltyPointsEarned,
    string? Notes,
    bool RequiresConsultation,
    DateTime? ConsultationScheduledAt,
    DateTime? ConsultationDoneAt,
    string? CancellationReason,
    DateTime? CancelledAt,
    DateTime? CompletedAt,
    DateTime CreatedAt,
    IReadOnlyList<BookingItemDto> Items,
    IReadOnlyList<PaymentDto> Payments
);

public sealed record BookingListDto(
    Guid Id,
    string BookingCode,
    string CustomerName,
    string StaffName,
    DateOnly BookingDate,
    TimeOnly BookingTime,
    BookingType BookingType,
    BookingStatus Status,
    decimal FinalAmount,
    int ItemCount,
    DateTime CreatedAt
);

public sealed record BookingItemDto(
    Guid Id,
    Guid ServiceId,
    string ServiceName,
    decimal Price,
    int DurationMinutes,
    int BufferMinutes,
    int LoyaltyPoints
);

public sealed record PaymentDto(
    Guid Id,
    decimal Amount,
    PaymentType PaymentType,
    PaymentMethod PaymentMethod,
    PaymentStatus Status,
    string? TransactionId,
    DateTime? PaidAt
);