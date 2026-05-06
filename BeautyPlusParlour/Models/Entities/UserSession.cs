namespace BeautyPlusParlour.Models.Entities;

public sealed class UserSession
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string RefreshTokenHash { get; private set; } = string.Empty;
    public string? ReplacedByTokenHash { get; private set; }  // ← NEW: token chain
    public string DeviceInfo { get; private set; } = string.Empty;
    public string Browser { get; private set; } = string.Empty;  // ← NEW
    public string IpAddress { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty;  // ← NEW
    public bool IsRevoked { get; private set; }
    public bool IsSuspicious { get; private set; }  // ← NEW: breach flag
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public User User { get; private set; } = null!;

    private UserSession() { }

    public static UserSession Create(
        Guid userId, string refreshTokenHash,
        string deviceInfo, string browser,
        string ipAddress, string location,
        DateTime expiresAt)
    {
        return new UserSession
        {
            UserId = userId,
            RefreshTokenHash = refreshTokenHash,
            DeviceInfo = deviceInfo.Length > 500 ? deviceInfo[..500] : deviceInfo,
            Browser = browser.Length > 200 ? browser[..200] : browser,
            IpAddress = ipAddress,
            Location = location,
            ExpiresAt = expiresAt
        };
    }

    public void Revoke(string? replacedByTokenHash = null)
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        ReplacedByTokenHash = replacedByTokenHash;
    }

    public void MarkSuspicious()
    {
        IsSuspicious = true;
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
    }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
}