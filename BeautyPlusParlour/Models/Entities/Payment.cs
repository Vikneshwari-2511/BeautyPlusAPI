using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.Entities;

public sealed class Payment
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid BookingId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentType PaymentType { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public string? TransactionId { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation
    public Booking Booking { get; private set; } = null!;

    private Payment() { }

    public static Payment Create(
        Guid bookingId, decimal amount,
        PaymentType paymentType,
        PaymentMethod paymentMethod,
        string? transactionId)
    {
        return new Payment
        {
            BookingId = bookingId,
            Amount = amount,
            PaymentType = paymentType,
            PaymentMethod = paymentMethod,
            TransactionId = transactionId,
            Status = PaymentStatus.Completed,
            PaidAt = DateTime.UtcNow
        };
    }

    public void MarkRefunded() => Status = PaymentStatus.Refunded;
}