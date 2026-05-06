using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.Entities;

public sealed class CustomerLoyaltyPoints
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid CustomerId { get; private set; }
    public int TotalPoints { get; private set; }
    public int TotalEarned { get; private set; }
    public int TotalRedeemed { get; private set; }
    public int TotalExpired { get; private set; }
    public LoyaltyTier Tier { get; private set; } = LoyaltyTier.Silver;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation
    public CustomerProfile Customer { get; private set; } = null!;

    private CustomerLoyaltyPoints() { }

    public static CustomerLoyaltyPoints Create(Guid customerId) =>
        new()
        {
            CustomerId = customerId,
            TotalPoints = 0,
            Tier = LoyaltyTier.Silver
        };

    public void AddPoints(int points)
    {
        TotalPoints += points;
        TotalEarned += points;
        RecalculateTier();
        UpdatedAt = DateTime.UtcNow;
    }

    public void RedeemPoints(int points)
    {
        if (points > TotalPoints)
            throw new InvalidOperationException(
                ResponseMessages.InsufficientPoints);

        TotalPoints -= points;
        TotalRedeemed += points;
        RecalculateTier();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ExpirePoints(int points)
    {
        var toExpire = Math.Min(points, TotalPoints);
        TotalPoints -= toExpire;
        TotalExpired += toExpire;
        RecalculateTier();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Adjust(int points)
    {
        TotalPoints = Math.Max(0, TotalPoints + points);
        RecalculateTier();
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal GetTierDiscountPercent() => Tier switch
    {
        LoyaltyTier.Silver => LoyaltyConstants.SilverDiscount,
        LoyaltyTier.Gold => LoyaltyConstants.GoldDiscount,
        LoyaltyTier.Diamond => LoyaltyConstants.DiamondDiscount,
        _ => 0m
    };

    private void RecalculateTier()
    {
        Tier = TotalPoints switch
        {
            >= LoyaltyConstants.DiamondMin => LoyaltyTier.Diamond,
            >= LoyaltyConstants.GoldMin => LoyaltyTier.Gold,
            _ => LoyaltyTier.Silver
        };
    }
}