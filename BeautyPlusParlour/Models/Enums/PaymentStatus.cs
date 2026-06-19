namespace BeautyPlusParlour.Models.Enums;

public enum PaymentStatus
{
    Created = 1,    // Order created, awaiting payment
    Captured = 2,   // Payment successful
    Failed = 3,     // Payment failed
    Refunded = 4    // Payment refunded
}