using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Data;
using BeautyPlusParlour.Exceptions;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Loyalty;
using BeautyPlusParlour.Models.Entities;
using BeautyPlusParlour.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BeautyPlusParlour.Services;

public sealed class LoyaltyService : ILoyaltyService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly ILogger<LoyaltyService> _logger;
    private readonly INotificationService _notifications;
    public LoyaltyService(
        AppDbContext db,
        IAuditService audit,
        ILogger<LoyaltyService> logger,
        INotificationService notifications)
    {
        _db = db;
        _audit = audit;
        _notifications = notifications;
        _logger = logger;
    }

    // ── Get my points ─────────────────────────────────────────────────────
    public async Task<LoyaltyPointsDto> GetMyPointsAsync(
        Guid userId, CancellationToken ct = default)
    {
        var customer = await GetCustomerByUserIdAsync(userId, ct);
        return await GetByCustomerIdAsync(customer.Id, ct);
    }

    public async Task<LoyaltyPointsDto> GetByCustomerIdAsync(
        Guid customerId, CancellationToken ct = default)
    {
        var points = await GetOrCreateLoyaltyAsync(customerId, ct);

        var customer = await _db.CustomerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == customerId, ct)
            ?? throw new NotFoundException(ResponseMessages.CustomerNotFound);

        return MapToDto(points, customer.FullName);
    }

    // ── Get my transactions ───────────────────────────────────────────────
    public async Task<IReadOnlyList<LoyaltyTransactionDto>> GetMyTransactionsAsync(
        Guid userId, CancellationToken ct = default)
    {
        var customer = await GetCustomerByUserIdAsync(userId, ct);

        var txns = await _db.LoyaltyTransactions
            .AsNoTracking()
            .Include(t => t.Booking)
            .Where(t => t.CustomerId == customer.Id)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

        return txns.Select(t => new LoyaltyTransactionDto(
            t.Id, t.TransactionType, t.Points,
            t.BalanceAfter, t.Description,
            t.Booking?.BookingCode,
            t.ExpiresAt, t.CreatedAt))
            .ToList().AsReadOnly();
    }

    // ── Validate redeem ───────────────────────────────────────────────────
    public async Task<ValidateRedeemResponse> ValidateRedeemAsync(
        Guid userId, ValidateRedeemRequest request,
        CancellationToken ct = default)
    {
        var customer = await GetCustomerByUserIdAsync(userId, ct);
        var loyalty = await GetOrCreateLoyaltyAsync(customer.Id, ct);

        if (request.PointsToRedeem < LoyaltyConstants.MinRedeemPoints)
            return new ValidateRedeemResponse(
    false,
    0, // PointsToRedeem
    0m, // DiscountAmount (decimal)
    loyalty.TotalPoints, // RemainingPoints
    ResponseMessages.InsufficientPoints // Message
);

        if (request.PointsToRedeem > loyalty.TotalPoints)
            return new ValidateRedeemResponse(
    false,
    0, // PointsToRedeem
    0m, // DiscountAmount (decimal)
    loyalty.TotalPoints, // RemainingPoints
    ResponseMessages.InsufficientPoints // Message
);

        // Max 20% of booking total can be redeemed
        var maxRedeemValue = Math.Round(
            request.BookingTotal *
            (LoyaltyConstants.MaxRedeemPercent / 100m), 2);

        var redeemValue = Math.Min(
            request.PointsToRedeem * LoyaltyConstants.PointsPerRupee,
            (int)maxRedeemValue);

        var actualPoints = redeemValue / LoyaltyConstants.PointsPerRupee;

        if (actualPoints < LoyaltyConstants.MinRedeemPoints)
            return new ValidateRedeemResponse(
    false,
    0, // PointsToRedeem
    0m, // DiscountAmount (decimal)
    loyalty.TotalPoints, // RemainingPoints
    ResponseMessages.InsufficientPoints // Message
);

        return new ValidateRedeemResponse(
    false,
    0, // PointsToRedeem
    0m, // DiscountAmount (decimal)
    loyalty.TotalPoints, // RemainingPoints
    ResponseMessages.InsufficientPoints // Message
);
    }

    // ── Adjust (Admin) ────────────────────────────────────────────────────
    public async Task<LoyaltyPointsDto> AdjustAsync(
        AdjustPointsRequest request, Guid adminId,
        CancellationToken ct = default)
    {
        var loyalty = await GetOrCreateLoyaltyAsync(request.CustomerId, ct);

        loyalty.Adjust(request.Points);

        var txn = LoyaltyTransaction.Create(
            request.CustomerId, null,
            LoyaltyTransactionType.Adjust,
            request.Points, loyalty.TotalPoints,
            $"Manual adjustment: {request.Description}");

        _db.LoyaltyTransactions.Add(txn);
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            adminId, AuditAction.Updated,
            "LoyaltyPoints", request.CustomerId.ToString(),
            null, $"Adjusted by {request.Points}: {request.Description}",
            ct: ct);

        _logger.LogInformation(
            "Loyalty points adjusted for CustomerId {Id}: {Points} by AdminId {AdminId}",
            request.CustomerId, request.Points, adminId);

        var customer = await _db.CustomerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId, ct)!;

        return MapToDto(loyalty, customer!.FullName);
    }

    // ── Get all customers (Admin) ─────────────────────────────────────────
    public async Task<IReadOnlyList<LoyaltyPointsDto>> GetAllCustomersAsync(
        CancellationToken ct = default)
    {
        var loyaltyList = await _db.CustomerLoyaltyPoints
            .AsNoTracking()
            .Include(l => l.Customer)
            .OrderByDescending(l => l.TotalPoints)
            .ToListAsync(ct);

        return loyaltyList
            .Select(l => MapToDto(l, l.Customer.FullName))
            .ToList().AsReadOnly();
    }

    // ── Credit earned points (called by BookingService) ───────────────────
    public async Task CreditEarnedPointsAsync(
        Guid customerId, Guid bookingId, int points,
        string description, CancellationToken ct = default)
    {
        var loyalty = await GetOrCreateLoyaltyAsync(customerId, ct);

        loyalty.AddPoints(points);

        var expiresAt = DateTime.UtcNow.AddMonths(LoyaltyConstants.ExpiryMonths);

        var txn = LoyaltyTransaction.Create(
            customerId, bookingId,
            LoyaltyTransactionType.Earn,
            points, loyalty.TotalPoints,
            description, expiresAt);

        _db.LoyaltyTransactions.Add(txn);
        await _db.SaveChangesAsync(ct);
        await _notifications.NotifyLoyaltyEarnedAsync(
    customerId, points, loyalty.Tier.ToString(), ct);
        _logger.LogInformation(
            "Loyalty earned: {Points} pts for CustomerId {Id}. Tier: {Tier}",
            points, customerId, loyalty.Tier);
    }

    // ── Debit redeemed points (called by BookingService) ──────────────────
    public async Task DebitRedeemedPointsAsync(
        Guid customerId, Guid bookingId, int points,
        string description, CancellationToken ct = default)
    {
        var loyalty = await GetOrCreateLoyaltyAsync(customerId, ct);

        if (points > loyalty.TotalPoints)
            throw new AppException(ResponseMessages.InsufficientPoints);

        loyalty.RedeemPoints(points);

        var txn = LoyaltyTransaction.Create(
            customerId, bookingId,
            LoyaltyTransactionType.Redeem,
            -points, loyalty.TotalPoints,
            description);

        _db.LoyaltyTransactions.Add(txn);
        await _db.SaveChangesAsync(ct);
    }

    // ── Expire old points (background job) ────────────────────────────────
    public async Task ExpireOldPointsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var expiredTxns = await _db.LoyaltyTransactions
            .Where(t =>
                t.TransactionType == LoyaltyTransactionType.Earn &&
                t.ExpiresAt.HasValue &&
                t.ExpiresAt.Value < now &&
                t.Points > 0)
            .ToListAsync(ct);

        foreach (var txn in expiredTxns)
        {
            var loyalty = await GetOrCreateLoyaltyAsync(txn.CustomerId, ct);

            loyalty.ExpirePoints(txn.Points);

            var expiry = LoyaltyTransaction.Create(
                txn.CustomerId, null,
                LoyaltyTransactionType.Expire,
                -txn.Points, loyalty.TotalPoints,
                $"Points expired (earned on {txn.CreatedAt:dd MMM yyyy})");

            _db.LoyaltyTransactions.Add(expiry);

            // Mark original as expired (set Points to 0 to avoid double expiry)
            // We track expiry via the separate Expire transaction
        }

        if (expiredTxns.Count > 0)
        {
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation(
                "Expired loyalty points for {Count} transactions.",
                expiredTxns.Count);
        }
    }

    // ── private ───────────────────────────────────────────────────────────
    private async Task<CustomerLoyaltyPoints> GetOrCreateLoyaltyAsync(
        Guid customerId, CancellationToken ct)
    {
        var loyalty = await _db.CustomerLoyaltyPoints
            .FirstOrDefaultAsync(l => l.CustomerId == customerId, ct);

        if (loyalty is not null) return loyalty;

        loyalty = CustomerLoyaltyPoints.Create(customerId);
        _db.CustomerLoyaltyPoints.Add(loyalty);
        await _db.SaveChangesAsync(ct);
        return loyalty;
    }

    private async Task<CustomerProfile> GetCustomerByUserIdAsync(
        Guid userId, CancellationToken ct)
    {
        return await _db.CustomerProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId, ct)
            ?? throw new NotFoundException(ResponseMessages.CustomerNotFound);
    }

    private static LoyaltyPointsDto MapToDto(
        CustomerLoyaltyPoints l, string customerName)
    {
        var (pointsToNext, nextTier) = l.Tier switch
        {
            LoyaltyTier.Silver => (LoyaltyConstants.GoldMin - l.TotalPoints, "Gold"),
            LoyaltyTier.Gold => (LoyaltyConstants.DiamondMin - l.TotalPoints, "Diamond"),
            LoyaltyTier.Diamond => (0, "Diamond (Max)"),
            _ => (0, "Unknown")
        };

        return new LoyaltyPointsDto(
            l.CustomerId, customerName,
            l.TotalPoints, l.TotalEarned,
            l.TotalRedeemed, l.TotalExpired,
            l.Tier, l.GetTierDiscountPercent(),
            Math.Max(0, pointsToNext), nextTier,
            l.UpdatedAt);
    }
}