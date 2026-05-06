using BeautyPlusParlour.Models.DTOs.Auth;

namespace BeautyPlusParlour.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(
        RegisterRequest request, CancellationToken ct = default);

    Task<AuthResponse> LoginAsync(
        LoginRequest request, string deviceInfo,
        string ipAddress, CancellationToken ct = default);

    Task LogoutAsync(Guid sessionId, CancellationToken ct = default);

    Task LogoutAllDevicesAsync(Guid userId, CancellationToken ct = default);

    Task VerifyEmailAsync(string token, CancellationToken ct = default);
    Task ResendVerificationEmailAsync(
    string email,
    CancellationToken ct = default);
}