using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.Entities;

public sealed class User
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int FailedLoginCount { get; private set; }
    public DateTime? LockedUntil { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    public ICollection<UserSession> Sessions { get; private set; } = [];
    public ICollection<OtpVerification> OtpVerifications { get; private set; } = [];
    public ICollection<EmailVerificationToken> EmailVerificationTokens { get; private set; } = [];
    public ICollection<PasswordResetToken> PasswordResetTokens { get; private set; } = [];

    private User() { }

    public static User Create(
        string fullName, string email,
        string passwordHash, string phoneNumber,
        UserRole role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return new User
        {
            FullName = fullName.Trim(),
            Email = email.ToLowerInvariant().Trim(),
            PasswordHash = passwordHash,
            PhoneNumber = phoneNumber.Trim(),
            Role = role
        };
    }

    public void VerifyEmail()
    {
        IsEmailVerified = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(string newHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newHash);
        PasswordHash = newHash;
        FailedLoginCount = 0;
        LockedUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }
    public void UpdateProfile(
    string fullName,
    string phoneNumber)
    {
        FullName = fullName.Trim();
        PhoneNumber = phoneNumber.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    public void RecordFailedLogin(int maxAttempts = 5, int lockoutMinutes = 15)
    {
        FailedLoginCount++;
        if (FailedLoginCount >= maxAttempts)
            LockedUntil = DateTime.UtcNow.AddMinutes(lockoutMinutes);
        UpdatedAt = DateTime.UtcNow;
    }

    public void ResetFailedLogin()
    {
        FailedLoginCount = 0;
        LockedUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsLockedOut() =>
        LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;
}