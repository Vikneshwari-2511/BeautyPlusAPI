using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.DTOs.Loyalty;

public sealed record LoyaltyPointsDto(
    Guid CustomerId,
    string CustomerName,
    int TotalPoints,
    int TotalEarned,
    int TotalRedeemed,
    int TotalExpired,
    LoyaltyTier Tier,
    decimal TierDiscountPercent,
    int PointsToNextTier,
    string NextTier,
    DateTime UpdatedAt
);

public sealed record LoyaltyTransactionDto(
    Guid Id,
    LoyaltyTransactionType TransactionType,
    int Points,
    int BalanceAfter,
    string Description,
    string? BookingCode,
    DateTime? ExpiresAt,
    DateTime CreatedAt
);

public sealed record ValidateRedeemRequest(
    int PointsToRedeem,
    decimal BookingTotal
);

public sealed record ValidateRedeemResponse(
    bool CanRedeem,
    int PointsToRedeem,
    decimal DiscountAmount,
    int RemainingPoints,
    string Message
);

public sealed record AdjustPointsRequest(
    Guid CustomerId,
    int Points,
    string Description
);

public sealed record LoyaltyExpirySummaryDto(
    Guid CustomerId,
    string CustomerName,
    int ExpiringPoints,
    DateTime ExpiryDate
);