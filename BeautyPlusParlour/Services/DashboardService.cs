using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Data;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Booking;
using BeautyPlusParlour.Models.DTOs.Dashboard;
using BeautyPlusParlour.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BeautyPlusParlour.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        AppDbContext db,
        ILogger<DashboardService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ── Summary ───────────────────────────────────────────────────────────
    public async Task<DashboardSummaryDto> GetSummaryAsync(
        CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateOnly(today.Year, today.Month, 1);

        // ── Today ──
        var todayBookings = await _db.Bookings
            .Where(b => b.BookingDate == today)
            .ToListAsync(ct);

        var todayRevenue = todayBookings
            .Where(b => b.Status == BookingStatus.Completed)
            .Sum(b => b.FinalAmount);

        var newCustomersToday = await _db.CustomerProfiles
            .CountAsync(c =>
                DateOnly.FromDateTime(c.CreatedAt) == today, ct);

        var today_summary = new TodaySummaryDto(
            TotalBookings: todayBookings.Count,
            PendingBookings: todayBookings.Count(b => b.Status == BookingStatus.Pending),
            ConfirmedBookings: todayBookings.Count(b => b.Status == BookingStatus.Confirmed),
            CompletedBookings: todayBookings.Count(b => b.Status == BookingStatus.Completed),
            CancelledBookings: todayBookings.Count(b => b.Status == BookingStatus.Cancelled),
            TotalRevenue: todayRevenue,
            NewCustomers: newCustomersToday);

        // ── Overall ──
        var overall = new OverallStatsDto(
            TotalCustomers: await _db.CustomerProfiles.CountAsync(ct),
            ActiveCustomers: await _db.CustomerProfiles.CountAsync(c => c.IsActive, ct),
            TotalStaff: await _db.StaffProfiles.CountAsync(ct),
            ActiveStaff: await _db.StaffProfiles.CountAsync(s => s.IsActive, ct),
            TotalServices: await _db.Services.CountAsync(ct),
            ActiveServices: await _db.Services.CountAsync(s => s.IsActive, ct),
            TotalCategories: await _db.Categories.CountAsync(c => c.IsActive, ct),
            TotalBookingsAllTime: await _db.Bookings.CountAsync(ct),
            TotalRevenueAllTime: await _db.Bookings
                .Where(b => b.Status == BookingStatus.Completed)
                .SumAsync(b => b.FinalAmount, ct));

        // ── Quick Stats ──
        var bookingsThisWeek = await _db.Bookings
            .CountAsync(b => b.BookingDate >= weekStart, ct);

        var bookingsThisMonth = await _db.Bookings
            .CountAsync(b => b.BookingDate >= monthStart, ct);

        var revenueThisWeek = await _db.Bookings
            .Where(b => b.BookingDate >= weekStart &&
                        b.Status == BookingStatus.Completed)
            .SumAsync(b => b.FinalAmount, ct);

        var revenueThisMonth = await _db.Bookings
            .Where(b => b.BookingDate >= monthStart &&
                        b.Status == BookingStatus.Completed)
            .SumAsync(b => b.FinalAmount, ct);

        var pendingLeaves = await _db.StaffLeaves
            .CountAsync(l => l.Status == LeaveStatus.Pending, ct);

        var quick = new QuickStatsDto(
            BookingsThisWeek: bookingsThisWeek,
            BookingsThisMonth: bookingsThisMonth,
            RevenueThisWeek: revenueThisWeek,
            RevenueThisMonth: revenueThisMonth,
            PendingLeaveRequests: pendingLeaves,
            PendingReviews: 0,
            UnreadNotifications: 0);

        return new DashboardSummaryDto(today_summary, overall, quick);
    }

    // ── Revenue ───────────────────────────────────────────────────────────
    public async Task<RevenueDto> GetRevenueAsync(
        string period, DateOnly? from, DateOnly? to,
        CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Set date range based on period
        var (startDate, endDate) = period.ToLower() switch
        {
            "week" => (today.AddDays(-6), today),
            "month" => (new DateOnly(today.Year, today.Month, 1), today),
            "year" => (new DateOnly(today.Year, 1, 1), today),
            "custom" when from.HasValue && to.HasValue => (from.Value, to.Value),
            _ => (today.AddDays(-29), today) // default 30 days
        };

        var completedBookings = await _db.Bookings
            .Where(b =>
                b.Status == BookingStatus.Completed &&
                b.BookingDate >= startDate &&
                b.BookingDate <= endDate)
            .ToListAsync(ct);

        // Group by date
        var grouped = completedBookings
            .GroupBy(b => b.BookingDate)
            .OrderBy(g => g.Key)
            .Select(g => new RevenuePeriodDto(
                Label: g.Key.ToString("dd MMM"),
                Date: g.Key,
                Revenue: g.Sum(b => b.FinalAmount),
                BookingCount: g.Count()))
            .ToList();

        // Fill in zero-revenue days
        var allDays = new List<RevenuePeriodDto>();
        for (var d = startDate; d <= endDate; d = d.AddDays(1))
        {
            var existing = grouped.FirstOrDefault(g => g.Date == d);
            allDays.Add(existing ?? new RevenuePeriodDto(
                d.ToString("dd MMM"), d, 0m, 0));
        }

        var total = allDays.Sum(d => d.Revenue);
        var nonZero = allDays.Where(d => d.Revenue > 0).ToList();

        return new RevenueDto(
            Data: allDays.AsReadOnly(),
            TotalRevenue: total,
            AverageRevenue: nonZero.Count > 0
                ? Math.Round(total / nonZero.Count, 2)
                : 0m,
            HighestRevenue: allDays.Count > 0
                ? allDays.Max(d => d.Revenue)
                : 0m,
            Period: period);
    }

    // ── Booking Analytics ─────────────────────────────────────────────────
    public async Task<BookingAnalyticsDto> GetBookingAnalyticsAsync(
        DateOnly? from, DateOnly? to,
        CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = from ?? today.AddDays(-29);
        var endDate = to ?? today;

        var bookings = await _db.Bookings
            .Where(b =>
                b.BookingDate >= startDate &&
                b.BookingDate <= endDate)
            .ToListAsync(ct);

        // Status breakdown
        var byStatus = new BookingStatusBreakdownDto(
            Pending: bookings.Count(b => b.Status == BookingStatus.Pending),
            Confirmed: bookings.Count(b => b.Status == BookingStatus.Confirmed),
            InProgress: bookings.Count(b => b.Status == BookingStatus.InProgress),
            Completed: bookings.Count(b => b.Status == BookingStatus.Completed),
            Cancelled: bookings.Count(b => b.Status == BookingStatus.Cancelled));

        // Type breakdown
        var byType = new BookingTypeBreakdownDto(
            Parlour: bookings.Count(b => b.BookingType == BookingType.Parlour),
            OnSite: bookings.Count(b => b.BookingType == BookingType.OnSite));

        // Daily trend
        var trend = new List<BookingTrendDto>();
        for (var d = startDate; d <= endDate; d = d.AddDays(1))
        {
            var count = bookings.Count(b => b.BookingDate == d);
            trend.Add(new BookingTrendDto(
                d.ToString("dd MMM"), d, count));
        }

        return new BookingAnalyticsDto(byStatus, byType, trend.AsReadOnly());
    }

    // ── Top Services ──────────────────────────────────────────────────────
    public async Task<IReadOnlyList<TopServiceDto>> GetTopServicesAsync(
        int top, CancellationToken ct = default)
    {
        // Get booking item counts per service
        var serviceStats = await _db.BookingItems
            .Include(i => i.Service)
                .ThenInclude(s => s.Category)
            .Include(i => i.Booking)
            .Where(i => i.Booking.Status == BookingStatus.Completed)
            .GroupBy(i => new
            {
                i.ServiceId,
                i.Service.Name,
                CategoryName = i.Service.Category.Name
            })
            .Select(g => new
            {
                g.Key.ServiceId,
                g.Key.Name,
                g.Key.CategoryName,
                BookingCount = g.Count(),
                TotalRevenue = g.Sum(i => i.Price)
            })
            .OrderByDescending(g => g.BookingCount)
            .Take(top)
            .ToListAsync(ct);

        var result = new List<TopServiceDto>();

        foreach (var stat in serviceStats)
        {
            var avgRating = await _db.Reviews
                .Where(r => r.ServiceId == stat.ServiceId && r.IsVisible)
                .AverageAsync(r => (double?)r.ServiceRating, ct) ?? 0;

            result.Add(new TopServiceDto(
                stat.ServiceId,
                stat.Name,
                stat.CategoryName,
                stat.BookingCount,
                stat.TotalRevenue,
                Math.Round(avgRating, 1)));
        }

        return result.AsReadOnly();
    }

    // ── Top Staff ─────────────────────────────────────────────────────────
    public async Task<IReadOnlyList<TopStaffDto>> GetTopStaffAsync(
        int top, CancellationToken ct = default)
    {
        var staffStats = await _db.Bookings
            .Include(b => b.Staff)
            .Where(b => b.Status == BookingStatus.Completed)
            .GroupBy(b => new
            {
                b.StaffId,
                b.Staff.FullName,
                b.Staff.EmployeeCode,
                b.Staff.Designation,
                b.Staff.ProfileImageUrl
            })
            .Select(g => new
            {
                g.Key.StaffId,
                g.Key.FullName,
                g.Key.EmployeeCode,
                g.Key.Designation,
                g.Key.ProfileImageUrl,
                CompletedBookings = g.Count()
            })
            .OrderByDescending(g => g.CompletedBookings)
            .Take(top)
            .ToListAsync(ct);

        var result = new List<TopStaffDto>();

        foreach (var stat in staffStats)
        {
            var totalBookings = await _db.Bookings
                .CountAsync(b => b.StaffId == stat.StaffId, ct);

            var avgRating = await _db.Reviews
                .Where(r => r.StaffId == stat.StaffId && r.IsVisible)
                .AverageAsync(r => (double?)r.StaffRating, ct) ?? 0;

            result.Add(new TopStaffDto(
                stat.StaffId,
                stat.FullName,
                stat.EmployeeCode,
                stat.Designation,
                stat.ProfileImageUrl,
                totalBookings,
                stat.CompletedBookings,
                Math.Round(avgRating, 1)));
        }

        return result.AsReadOnly();
    }

    // ── Recent Bookings ───────────────────────────────────────────────────
    public async Task<IReadOnlyList<BookingListDto>> GetRecentBookingsAsync(
        int count, CancellationToken ct = default)
    {
        var bookings = await _db.Bookings
            .AsNoTracking()
            .Include(b => b.Customer)
            .Include(b => b.Staff)
            .Include(b => b.Items)
            .OrderByDescending(b => b.CreatedAt)
            .Take(count)
            .ToListAsync(ct);

        return bookings.Select(b => new BookingListDto(
            b.Id, b.BookingCode,
            b.Customer.FullName,
            b.Staff.FullName,
            b.BookingDate, b.BookingTime,
            b.BookingType, b.Status,
            b.FinalAmount, b.Items.Count,
            b.CreatedAt))
            .ToList().AsReadOnly();
    }

    // ── Customer Analytics ────────────────────────────────────────────────
    public async Task<CustomerAnalyticsDto> GetCustomerAnalyticsAsync(
        CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateOnly(today.Year, today.Month, 1);

        var total = await _db.CustomerProfiles.CountAsync(ct);

        var newThisWeek = await _db.CustomerProfiles
            .CountAsync(c =>
                DateOnly.FromDateTime(c.CreatedAt) >= weekStart, ct);

        var newThisMonth = await _db.CustomerProfiles
            .CountAsync(c =>
                DateOnly.FromDateTime(c.CreatedAt) >= monthStart, ct);

        // Repeat customers = those with more than 1 completed booking
        var repeatCustomers = await _db.Bookings
            .Where(b => b.Status == BookingStatus.Completed)
            .GroupBy(b => b.CustomerId)
            .CountAsync(g => g.Count() > 1, ct);

        // 6-month growth trend
        var trend = new List<CustomerGrowthDto>();
        var cumulative = 0;

        for (var i = 5; i >= 0; i--)
        {
            var monthDate = today.AddMonths(-i);
            var mStart = new DateOnly(monthDate.Year, monthDate.Month, 1);
            var mEnd = mStart.AddMonths(1).AddDays(-1);

            var newInMonth = await _db.CustomerProfiles
                .CountAsync(c =>
                    DateOnly.FromDateTime(c.CreatedAt) >= mStart &&
                    DateOnly.FromDateTime(c.CreatedAt) <= mEnd, ct);

            cumulative += newInMonth;

            trend.Add(new CustomerGrowthDto(
                monthDate.ToString("MMM yyyy"),
                mStart,
                newInMonth,
                cumulative));
        }

        return new CustomerAnalyticsDto(
            total, newThisMonth, newThisWeek,
            repeatCustomers, trend.AsReadOnly());
    }

    // ── Loyalty Analytics ─────────────────────────────────────────────────
    public async Task<LoyaltyAnalyticsDto> GetLoyaltyAnalyticsAsync(
        CancellationToken ct = default)
    {
        var totalSilver = await _db.CustomerLoyaltyPoints
            .CountAsync(l => l.Tier == LoyaltyTier.Silver, ct);

        var totalGold = await _db.CustomerLoyaltyPoints
            .CountAsync(l => l.Tier == LoyaltyTier.Gold, ct);

        var totalDiamond = await _db.CustomerLoyaltyPoints
            .CountAsync(l => l.Tier == LoyaltyTier.Diamond, ct);

        var totalIssued = await _db.CustomerLoyaltyPoints
            .SumAsync(l => l.TotalEarned, ct);

        var totalRedeemed = await _db.CustomerLoyaltyPoints
            .SumAsync(l => l.TotalRedeemed, ct);

        var totalExpired = await _db.CustomerLoyaltyPoints
            .SumAsync(l => l.TotalExpired, ct);

        // Total discount given via loyalty redemptions
        var totalDiscount = await _db.LoyaltyTransactions
            .Where(t => t.TransactionType == LoyaltyTransactionType.Redeem)
            .SumAsync(t => (decimal)Math.Abs(t.Points) *
                LoyaltyConstants.PointsPerRupee, ct);

        return new LoyaltyAnalyticsDto(
            totalSilver, totalGold, totalDiamond,
            totalIssued, totalRedeemed, totalExpired,
            totalDiscount);
    }
}