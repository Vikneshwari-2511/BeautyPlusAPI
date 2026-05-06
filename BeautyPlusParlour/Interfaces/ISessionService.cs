using BeautyPlusParlour.Models.DTOs.Auth;
using BeautyPlusParlour.Models.Entities;

namespace BeautyPlusParlour.Interfaces;

public interface ISessionService
{
    Task<UserSession> CreateAsync(
        Guid userId, string tokenHash,
        string deviceInfo, string browser,
        string ipAddress, string location,
        DateTime expiresAt, CancellationToken ct = default);

    Task<UserSession?> GetActiveByTokenHashAsync(
        string tokenHash, CancellationToken ct = default);

    // Returns the revoked session if the hash was previously used
    // (breach detection — token reuse)
    Task<UserSession?> GetRevokedByTokenHashAsync(
        string tokenHash, CancellationToken ct = default);

    Task RevokeAsync(
        Guid sessionId,
        string? replacedByTokenHash = null,
        CancellationToken ct = default);

    Task RevokeAllAsync(
        Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<SessionDto>> GetActiveSessionsAsync(
        Guid userId, Guid? currentSessionId,
        CancellationToken ct = default);
}