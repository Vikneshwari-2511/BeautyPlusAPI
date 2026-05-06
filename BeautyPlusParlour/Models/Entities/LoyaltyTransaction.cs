using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.Entities;

public sealed class LoyaltyTransaction
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid CustomerId { get; private set; }
    public Guid? BookingId { get; private set; }
    public LoyaltyTransactionType TransactionType { get; private set; }
    public int Points { get; private set; }
    public int BalanceAfter { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime? ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation
    public CustomerProfile Customer { get; private set; } = null!;
    public Booking? Booking { get; private set; }

    private LoyaltyTransaction() { }

    public static LoyaltyTransaction Create(
        Guid customerId, Guid? bookingId,
        LoyaltyTransactionType type, int points,
        int balanceAfter, string description,
        DateTime? expiresAt = null)
    {
        return new LoyaltyTransaction
        {
            CustomerId = customerId,
            BookingId = bookingId,
            TransactionType = type,
            Points = points,
            BalanceAfter = balanceAfter,
            Description = description.Trim(),
            ExpiresAt = expiresAt
        };
    }
}