namespace BeautyPlusParlour.Models.DTOs.Auth;

public sealed record SessionDto(
    Guid SessionId,
    string DeviceInfo,
    string Browser,
    string IpAddress,
    string Location,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    bool IsActive,
    bool IsCurrent    // true when sessionId matches caller's session
);