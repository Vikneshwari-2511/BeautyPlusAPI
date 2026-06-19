// Models/Entities/Payment.cs
using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.Entities;

public class Payment
{
    public Guid Id { get; private set; }
    public Guid BookingId { get; private set; }

    // Razorpay fields (online payments)
    public string RazorpayOrderId { get; private set; } = string.Empty;
    public string? RazorpayPaymentId { get; private set; }
    public string Currency { get; private set; } = "INR";

    // Shared fields
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? Method { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Manual/cash payment fields
    public PaymentType? PaymentType { get; private set; }
    public PaymentMethod? PaymentMethod { get; private set; }
    public string? TransactionId { get; private set; }

    // Navigation
    public Booking Booking { get; private set; } = null!;

    private Payment() { }

    // ── Factory: Razorpay online payment ─────────────────
    public static Payment Create(
        Guid bookingId,
        string razorpayOrderId,
        decimal amount,
        string currency = "INR")
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            RazorpayOrderId = razorpayOrderId,
            Amount = amount,
            Currency = currency,
            Status = PaymentStatus.Created,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    // ── Factory: Manual/cash payment (BookingService) ────
    // ── Factory: Manual/cash payment (BookingService) ────
    public static Payment Create(
        Guid bookingId,
        decimal amount,
        PaymentType? paymentType,      // ← nullable
        PaymentMethod? paymentMethod,  // ← nullable
        string? transactionId = null)
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            RazorpayOrderId = string.Empty,
            Amount = amount,
            PaymentType = paymentType,
            PaymentMethod = paymentMethod,
            TransactionId = transactionId,
            Status = PaymentStatus.Captured,
            PaidAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    public void MarkCaptured(string razorpayPaymentId, DateTime paidAt)
    {
        RazorpayPaymentId = razorpayPaymentId;
        Status = PaymentStatus.Captured;
        PaidAt = paidAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        Status = PaymentStatus.Failed;
        FailureReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }
}