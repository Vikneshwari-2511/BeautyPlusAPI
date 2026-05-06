using BeautyPlusParlour.Models.DTOs.Booking;
using BeautyPlusParlour.Models.DTOs.Dashboard;

namespace BeautyPlusParlour.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default);
    Task<RevenueDto> GetRevenueAsync(string period, DateOnly? from, DateOnly? to, CancellationToken ct = default);
    Task<BookingAnalyticsDto> GetBookingAnalyticsAsync(DateOnly? from, DateOnly? to, CancellationToken ct = default);
    Task<IReadOnlyList<TopServiceDto>> GetTopServicesAsync(int top, CancellationToken ct = default);
    Task<IReadOnlyList<TopStaffDto>> GetTopStaffAsync(int top, CancellationToken ct = default);
    Task<IReadOnlyList<BookingListDto>> GetRecentBookingsAsync(int count, CancellationToken ct = default);
    Task<CustomerAnalyticsDto> GetCustomerAnalyticsAsync(CancellationToken ct = default);
    Task<LoyaltyAnalyticsDto> GetLoyaltyAnalyticsAsync(CancellationToken ct = default);
}