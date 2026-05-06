using BeautyPlusParlour.Data;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Auth;
using BeautyPlusParlour.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeautyPlusParlour.Services;

public sealed class SessionService : ISessionService
{
    private readonly AppDbContext _db;

    public SessionService(AppDbContext db) => _db = db;

    public async Task<UserSession> CreateAsync(
        Guid userId, string tokenHash,
        string deviceInfo, string browser,
        string ipAddress, string location,
        DateTime expiresAt, CancellationToken ct = default)
    {
        var session = UserSession.Create(
            userId, tokenHash,
            deviceInfo, browser,
            ipAddress, location,
            expiresAt);

        _db.UserSessions.Add(session);
        await _db.SaveChangesAsync(ct);
        return session;
    }

    public async Task<UserSession?> GetActiveByTokenHashAsync(
        string tokenHash, CancellationToken ct = default)
    {
        return await _db.UserSessions
            .FirstOrDefaultAsync(
                s => s.RefreshTokenHash == tokenHash
                  && !s.IsRevoked
                  && s.ExpiresAt > DateTime.UtcNow, ct);
    }

    // Used for breach detection — finds a session where this
    // token was already rotated away (it lives in ReplacedByTokenHash)
    public async Task<UserSession?> GetRevokedByTokenHashAsync(
        string tokenHash, CancellationToken ct = default)
    {
        return await _db.UserSessions
            .FirstOrDefaultAsync(
                s => s.RefreshTokenHash == tokenHash
                  && s.IsRevoked, ct);
    }

    public async Task<IReadOnlyList<SessionDto>> GetActiveSessionsAsync(
        Guid userId, Guid? currentSessionId,
        CancellationToken ct = default)
    {
        var sessions = await _db.UserSessions
            .AsNoTracking()
            .Where(s => s.UserId == userId
                     && !s.IsRevoked
                     && s.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SessionDto(
                s.Id,
                s.DeviceInfo,
                s.Browser,
                s.IpAddress,
                s.Location,
                s.CreatedAt,
                s.ExpiresAt,
                true,
                s.Id == currentSessionId))
            .ToListAsync(ct);

        return sessions.AsReadOnly();
    }

    public async Task RevokeAsync(
        Guid sessionId,
        string? replacedByTokenHash = null,
        CancellationToken ct = default)
    {
        var session = await _db.UserSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct);

        if (session is null) return;

        session.Revoke(replacedByTokenHash);
        await _db.SaveChangesAsync(ct);
    }

    public async Task RevokeAllAsync(
        Guid userId, CancellationToken ct = default)
    {
        var sessions = await _db.UserSessions
            .Where(s => s.UserId == userId && !s.IsRevoked)
            .ToListAsync(ct);

        foreach (var s in sessions)
            s.Revoke();

        await _db.SaveChangesAsync(ct);
    }
}