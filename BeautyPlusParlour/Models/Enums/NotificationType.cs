namespace BeautyPlusParlour.Models.Enums;

public static class NotificationType
{
    public const string BookingCreated = "BookingCreated";
    public const string BookingConfirmed = "BookingConfirmed";
    public const string BookingStarted = "BookingStarted";
    public const string BookingCompleted = "BookingCompleted";
    public const string BookingCancelled = "BookingCancelled";
    public const string BookingRescheduled = "BookingRescheduled";
    public const string LeaveApproved = "LeaveApproved";
    public const string LeaveRejected = "LeaveRejected";
    public const string LoyaltyEarned = "LoyaltyEarned";
    public const string LoyaltyTierUpgrade = "LoyaltyTierUpgrade";
    public const string ReviewReminder = "ReviewReminder";
    public const string General = "General";
}

public static class ReferenceType
{
    public const string Booking = "Booking";
    public const string Leave = "Leave";
    public const string Review = "Review";
    public const string Loyalty = "Loyalty";
}