namespace BeautyPlusParlour.Constants;

public static class LoyaltyConstants
{
    // Tier thresholds
    public const int SilverMin = 0;
    public const int GoldMin = 200;
    public const int DiamondMin = 500;

    // Tier discounts (%)
    public const decimal SilverDiscount = 5m;
    public const decimal GoldDiscount = 10m;
    public const decimal DiamondDiscount = 15m;

    // Point expiry
    public const int ExpiryMonths = 3;

    // Redemption
    public const int PointsPerRupee = 1;   // 1 point = ₹1 discount
    public const int MinRedeemPoints = 50;  // minimum to redeem at once
    public const int MaxRedeemPercent = 20;  // max 20% of bill via points
}