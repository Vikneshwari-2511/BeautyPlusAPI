namespace BeautyPlusParlour.Models.Entities;

public sealed class OtpVerification
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string OtpHash { get; private set; } = string.Empty;
    public string Purpose { get; private set; } = string.Empty;
    public bool IsUsed { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; private set; }

    public User User { get; private set; } = null!;

    private OtpVerification() { }

    public static OtpVerification Create(
        Guid userId, string otpHash,
        string purpose, int expiryMinutes = 10)
    {
        return new OtpVerification
        {
            UserId = userId,
            OtpHash = otpHash,
            Purpose = purpose,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes)
        };
    }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
    public void MarkUsed() => IsUsed = true;
}