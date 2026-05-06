namespace BeautyPlusParlour.Models.Entities;

public sealed class EmailVerificationToken
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public bool IsUsed { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; private set; }

    public User User { get; private set; } = null!;

    private EmailVerificationToken() { }

    public static EmailVerificationToken Create(
        Guid userId, string token, int expiryHours = 24)
    {
        return new EmailVerificationToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(expiryHours)
        };
    }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
    public void MarkUsed() => IsUsed = true;
}