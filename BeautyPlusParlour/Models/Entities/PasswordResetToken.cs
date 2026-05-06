namespace BeautyPlusParlour.Models.Entities;

public sealed class PasswordResetToken
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public bool IsUsed { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; private set; }

    public User User { get; private set; } = null!;

    private PasswordResetToken() { }

    public static PasswordResetToken Create(
        Guid userId, string tokenHash, int expiryMinutes = 30)
    {
        return new PasswordResetToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes)
        };
    }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
    public void MarkUsed() => IsUsed = true;
}