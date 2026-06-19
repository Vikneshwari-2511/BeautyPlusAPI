namespace BeautyPlusParlour.Interfaces;

public interface IOtpService
{
    Task<string> GenerateAndStoreAsync(
        string email,
        string purpose,
        CancellationToken ct = default);

    Task<bool> ValidateAsync(
        string email,
        string purpose,
        string otp,
        CancellationToken ct = default);

    Task InvalidateAsync(
        string email,
        string purpose,
        CancellationToken ct = default);
}