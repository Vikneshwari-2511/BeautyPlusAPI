using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Booking;
using BeautyPlusParlour.Models.DTOs.Common;
using BeautyPlusParlour.Models.DTOs.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeautyPlusParlour.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AppRoles.AdminOnly)]
[Produces("application/json")]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service) =>
        _service = service;

    // ── GET /api/dashboard/summary ────────────────────────────────────────
    /// <summary>
    /// Returns today's stats, overall platform stats,
    /// and quick metrics for the admin home screen.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<DashboardSummaryDto>), 200)]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        var result = await _service.GetSummaryAsync(ct);
        return Ok(ApiResponse<DashboardSummaryDto>.Ok(
            result, ResponseMessages.DashboardSummaryFetched));
    }

    // ── GET /api/dashboard/revenue ────────────────────────────────────────
    /// <summary>
    /// Revenue by period.
    /// period: week | month | year | custom
    /// For custom, provide from and to query params.
    /// </summary>
    [HttpGet("revenue")]
    [ProducesResponseType(typeof(ApiResponse<RevenueDto>), 200)]
    public async Task<IActionResult> GetRevenue(
        [FromQuery] string period = "month",
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        CancellationToken ct = default)
    {
        var result = await _service.GetRevenueAsync(period, from, to, ct);
        return Ok(ApiResponse<RevenueDto>.Ok(
            result, ResponseMessages.RevenueDataFetched));
    }

    // ── GET /api/dashboard/bookings ───────────────────────────────────────
    /// <summary>
    /// Booking analytics — status breakdown,
    /// type breakdown, and daily trend.
    /// </summary>
    [HttpGet("bookings")]
    [ProducesResponseType(typeof(ApiResponse<BookingAnalyticsDto>), 200)]
    public async Task<IActionResult> GetBookingAnalytics(
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        CancellationToken ct = default)
    {
        var result = await _service.GetBookingAnalyticsAsync(from, to, ct);
        return Ok(ApiResponse<BookingAnalyticsDto>.Ok(
            result, ResponseMessages.BookingAnalyticsFetched));
    }

    // ── GET /api/dashboard/top-services ──────────────────────────────────
    /// <summary>
    /// Most booked services with revenue and average rating.
    /// top: number of services to return (default 10)
    /// </summary>
    [HttpGet("top-services")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TopServiceDto>>), 200)]
    public async Task<IActionResult> GetTopServices(
        [FromQuery] int top = 10,
        CancellationToken ct = default)
    {
        var result = await _service.GetTopServicesAsync(top, ct);
        return Ok(ApiResponse<IReadOnlyList<TopServiceDto>>.Ok(
            result, ResponseMessages.TopServicesFetched));
    }

    // ── GET /api/dashboard/top-staff ─────────────────────────────────────
    /// <summary>
    /// Highest performing staff by completed bookings and rating.
    /// top: number of staff to return (default 5)
    /// </summary>
    [HttpGet("top-staff")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TopStaffDto>>), 200)]
    public async Task<IActionResult> GetTopStaff(
        [FromQuery] int top = 5,
        CancellationToken ct = default)
    {
        var result = await _service.GetTopStaffAsync(top, ct);
        return Ok(ApiResponse<IReadOnlyList<TopStaffDto>>.Ok(
            result, ResponseMessages.TopStaffFetched));
    }

    // ── GET /api/dashboard/recent-bookings ────────────────────────────────
    /// <summary>
    /// Latest bookings across all customers and staff.
    /// count: number of bookings to return (default 10)
    /// </summary>
    [HttpGet("recent-bookings")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<BookingListDto>>), 200)]
    public async Task<IActionResult> GetRecentBookings(
        [FromQuery] int count = 10,
        CancellationToken ct = default)
    {
        var result = await _service.GetRecentBookingsAsync(count, ct);
        return Ok(ApiResponse<IReadOnlyList<BookingListDto>>.Ok(
            result, ResponseMessages.RecentBookingsFetched));
    }

    // ── GET /api/dashboard/customers ─────────────────────────────────────
    /// <summary>
    /// Customer growth analytics with 6-month trend.
    /// </summary>
    [HttpGet("customers")]
    [ProducesResponseType(typeof(ApiResponse<CustomerAnalyticsDto>), 200)]
    public async Task<IActionResult> GetCustomerAnalytics(CancellationToken ct)
    {
        var result = await _service.GetCustomerAnalyticsAsync(ct);
        return Ok(ApiResponse<CustomerAnalyticsDto>.Ok(
            result, ResponseMessages.CustomerAnalyticsFetched));
    }

    // ── GET /api/dashboard/loyalty ────────────────────────────────────────
    /// <summary>
    /// Loyalty tier distribution and points summary.
    /// </summary>
    [HttpGet("loyalty")]
    [ProducesResponseType(typeof(ApiResponse<LoyaltyAnalyticsDto>), 200)]
    public async Task<IActionResult> GetLoyaltyAnalytics(CancellationToken ct)
    {
        var result = await _service.GetLoyaltyAnalyticsAsync(ct);
        return Ok(ApiResponse<LoyaltyAnalyticsDto>.Ok(
            result, ResponseMessages.LoyaltyAnalyticsFetched));
    }
}