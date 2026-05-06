namespace BeautyPlusParlour.Models.DTOs.Notification;

public sealed record NotificationDto(
    Guid Id,
    string Title,
    string Body,
    string Type,
    Guid? ReferenceId,
    string? ReferenceType,
    bool IsRead,
    DateTime? ReadAt,
    DateTime CreatedAt
);

public sealed record UnreadCountDto(int Count);