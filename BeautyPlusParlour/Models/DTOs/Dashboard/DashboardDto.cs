using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.DTOs.Dashboard;

// ── Summary ──────────────────────────────────────────────────────────────

public sealed record DashboardSummaryDto(
    TodaySummaryDto Today,
    OverallStatsDto Overall,
    QuickStatsDto QuickStats
);

public sealed record TodaySummaryDto(
    int TotalBookings,
    int PendingBookings,
    int ConfirmedBookings,
    int CompletedBookings,
    int CancelledBookings,
    decimal TotalRevenue,
    int NewCustomers
);

public sealed record OverallStatsDto(
    int TotalCustomers,
    int ActiveCustomers,
    int TotalStaff,
    int ActiveStaff,
    int TotalServices,
    int ActiveServices,
    int TotalCategories,
    int TotalBookingsAllTime,
    decimal TotalRevenueAllTime
);

public sealed record QuickStatsDto(
    int BookingsThisWeek,
    int BookingsThisMonth,
    decimal RevenueThisWeek,
    decimal RevenueThisMonth,
    int PendingLeaveRequests,
    int PendingReviews,
    int UnreadNotifications
);

// ── Revenue ───────────────────────────────────────────────────────────────

public sealed record RevenueDto(
    IReadOnlyList<RevenuePeriodDto> Data,
    decimal TotalRevenue,
    decimal AverageRevenue,
    decimal HighestRevenue,
    string Period
);

public sealed record RevenuePeriodDto(
    string Label,
    DateOnly Date,
    decimal Revenue,
    int BookingCount
);

// ── Booking Analytics ─────────────────────────────────────────────────────

public sealed record BookingAnalyticsDto(
    BookingStatusBreakdownDto ByStatus,
    BookingTypeBreakdownDto ByType,
    IReadOnlyList<BookingTrendDto> Trend
);

public sealed record BookingStatusBreakdownDto(
    int Pending,
    int Confirmed,
    int InProgress,
    int Completed,
    int Cancelled
);

public sealed record BookingTypeBreakdownDto(
    int Parlour,
    int OnSite
);

public sealed record BookingTrendDto(
    string Label,
    DateOnly Date,
    int Count
);

// ── Top Services ──────────────────────────────────────────────────────────

public sealed record TopServiceDto(
    Guid ServiceId,
    string ServiceName,
    string CategoryName,
    int BookingCount,
    decimal TotalRevenue,
    double AverageRating
);

// ── Top Staff ─────────────────────────────────────────────────────────────

public sealed record TopStaffDto(
    Guid StaffId,
    string FullName,
    string EmployeeCode,
    Designation Designation,
    string? ProfileImageUrl,
    int BookingCount,
    int CompletedBookings,
    double AverageRating
);

// ── Customer Analytics ────────────────────────────────────────────────────

public sealed record CustomerAnalyticsDto(
    int TotalCustomers,
    int NewThisMonth,
    int NewThisWeek,
    int RepeatCustomers,
    IReadOnlyList<CustomerGrowthDto> GrowthTrend
);

public sealed record CustomerGrowthDto(
    string Label,
    DateOnly Date,
    int NewCustomers,
    int TotalCumulative
);

// ── Loyalty Analytics ─────────────────────────────────────────────────────

public sealed record LoyaltyAnalyticsDto(
    int TotalSilver,
    int TotalGold,
    int TotalDiamond,
    int TotalPointsIssued,
    int TotalPointsRedeemed,
    int TotalPointsExpired,
    decimal TotalDiscountGiven
);