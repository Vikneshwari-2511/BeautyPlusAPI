namespace BeautyPlusParlour.Interfaces;

public interface IOtpService
{
    Task<string> GenerateAsync(
        Guid userId, string purpose, CancellationToken ct = default);

    Task<bool> VerifyAsync(
        Guid userId, string otp,
        string purpose, CancellationToken ct = default);
}