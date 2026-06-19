namespace BeautyPlusParlour.Models.DTOs.Payment;

public sealed record CreatePaymentOrderRequest(
    Guid BookingId
);

public sealed record CreatePaymentOrderResponse(
    string OrderId,
    string RazorpayKeyId,
    decimal Amount,
    string Currency,
    string BookingCode,
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    string Description
);

public sealed record VerifyPaymentRequest(
    string RazorpayOrderId,
    string RazorpayPaymentId,
    string RazorpaySignature,
    Guid BookingId
);

public sealed record VerifyPaymentResponse(
    bool Success,
    string Message,
    string BookingCode,
    string BookingStatus
);

public sealed record PaymentDetailsDto(
    Guid Id,
    Guid BookingId,
    string BookingCode,
    string RazorpayOrderId,
    string? RazorpayPaymentId,
    decimal Amount,
    string Currency,
    string Status,
    string Method,
    string? FailureReason,
    DateTime CreatedAt,
    DateTime? PaidAt
);