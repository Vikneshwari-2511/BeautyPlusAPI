using BeautyPlusParlour.Models.DTOs.Loyalty;

namespace BeautyPlusParlour.Interfaces;

public interface ILoyaltyService
{
    Task<LoyaltyPointsDto> GetMyPointsAsync(Guid userId, CancellationToken ct = default);
    Task<LoyaltyPointsDto> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task<IReadOnlyList<LoyaltyTransactionDto>> GetMyTransactionsAsync(Guid userId, CancellationToken ct = default);
    Task<ValidateRedeemResponse> ValidateRedeemAsync(Guid userId, ValidateRedeemRequest request, CancellationToken ct = default);
    Task<LoyaltyPointsDto> AdjustAsync(AdjustPointsRequest request, Guid adminId, CancellationToken ct = default);
    Task<IReadOnlyList<LoyaltyPointsDto>> GetAllCustomersAsync(CancellationToken ct = default);

    // Called internally by BookingService on completion
    Task CreditEarnedPointsAsync(Guid customerId, Guid bookingId, int points, string description, CancellationToken ct = default);
    Task DebitRedeemedPointsAsync(Guid customerId, Guid bookingId, int points, string description, CancellationToken ct = default);

    // Called by background job
    Task ExpireOldPointsAsync(CancellationToken ct = default);
}