using BeautyPlusParlour.Models.DTOs.Auth;
using BeautyPlusParlour.Models.Entities;

namespace BeautyPlusParlour.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user, Guid sessionId);
    (string rawToken, string tokenHash) GenerateRefreshToken();
    Task<AuthResponse> RefreshAsync(
        string refreshToken, string deviceInfo,
        string ipAddress, CancellationToken ct = default);
}