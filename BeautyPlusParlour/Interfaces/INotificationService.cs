using BeautyPlusParlour.Models.DTOs.Notification;

namespace BeautyPlusParlour.Interfaces;

public interface INotificationService
{
    // In-app
    Task<IReadOnlyList<NotificationDto>> GetAllAsync(Guid userId, CancellationToken ct = default);
    Task<UnreadCountDto> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);
    Task MarkReadAsync(Guid notificationId, Guid userId, CancellationToken ct = default);
    Task MarkAllReadAsync(Guid userId, CancellationToken ct = default);
    Task DeleteAsync(Guid notificationId, Guid userId, CancellationToken ct = default);

    // Triggered internally by other modules
    Task NotifyBookingCreatedAsync(Guid bookingId, CancellationToken ct = default);
    Task NotifyBookingConfirmedAsync(Guid bookingId, CancellationToken ct = default);
    Task NotifyBookingStartedAsync(Guid bookingId, CancellationToken ct = default);
    Task NotifyBookingCompletedAsync(Guid bookingId, CancellationToken ct = default);
    Task NotifyBookingCancelledAsync(Guid bookingId, CancellationToken ct = default);
    Task NotifyBookingRescheduledAsync(Guid bookingId, CancellationToken ct = default);
    Task NotifyLeaveApprovedAsync(Guid leaveId, CancellationToken ct = default);
    Task NotifyLeaveRejectedAsync(Guid leaveId, CancellationToken ct = default);
    Task NotifyLoyaltyEarnedAsync(Guid customerId, int points, string tier, CancellationToken ct = default);
}