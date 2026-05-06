using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BeautyPlusParlour.Configurations;
using BeautyPlusParlour.Data;
using BeautyPlusParlour.Exceptions;
using BeautyPlusParlour.Helpers;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Auth;
using BeautyPlusParlour.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BeautyPlusParlour.Services;

public sealed class TokenService : ITokenService
{
    private readonly JwtSettings _jwt;
    private readonly AppDbContext _db;
    private readonly ISessionService _sessions;
    private readonly ILogger<TokenService> _logger;

    public TokenService(
        IOptions<JwtSettings> jwt,
        AppDbContext db,
        ISessionService sessions,
        ILogger<TokenService> logger)
    {
        _jwt = jwt.Value;
        _db = db;
        _sessions = sessions;
        _logger = logger;
    }

    public string GenerateAccessToken(User user, Guid sessionId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email,           user.Email),
            new Claim(ClaimTypes.Role,            user.Role.ToString()),
            new Claim("sid",                      sessionId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string rawToken, string tokenHash) GenerateRefreshToken()
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hash = HashHelper.ToSha256Base64(raw);
        return (raw, hash);
    }

    public async Task<AuthResponse> RefreshAsync(
    string refreshToken, string deviceInfo,
    string ipAddress, CancellationToken ct = default)
    {
        var tokenHash = HashHelper.ToSha256Base64(refreshToken);
        var browser = UserAgentHelper.ParseBrowser(deviceInfo);

        // ── Step 1: Look for an active session ───────────────────────────────
        var activeSession = await _sessions.GetActiveByTokenHashAsync(tokenHash, ct);

        if (activeSession is null)
        {
            // ── Step 2: Already rotated → BREACH DETECTED ────────────────────
            var revokedSession = await _sessions.GetRevokedByTokenHashAsync(tokenHash, ct);

            if (revokedSession is not null)
            {
                _logger.LogWarning(
                    "Refresh token reuse detected for UserId {UserId}. " +
                    "IP: {Ip}. Revoking all sessions.",
                    revokedSession.UserId, ipAddress);

                await _sessions.RevokeAllAsync(revokedSession.UserId, ct);

                throw new UnauthorizedException(
                    "Security alert: suspicious activity detected. " +
                    "All sessions have been terminated. Please log in again.");
            }

            throw new UnauthorizedException("Invalid or expired refresh token.");
        }

        // ── Step 3: Token is active but expired ──────────────────────────────
        if (activeSession.IsExpired())
        {
            await _sessions.RevokeAsync(activeSession.Id, ct: ct);
            throw new UnauthorizedException("Refresh token expired. Please log in again.");
        }

        // ── Step 4: Validate user still active ───────────────────────────────
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.Id == activeSession.UserId && u.IsActive, ct)
            ?? throw new UnauthorizedException("User not found or deactivated.");

        // ── Step 5: Rotate — revoke old, create new ───────────────────────────
        var (newRaw, newHash) = GenerateRefreshToken();
        var refreshExpiry = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays);

        // Revoke old session, store replacement hash for breach tracing
        await _sessions.RevokeAsync(
            activeSession.Id,
            replacedByTokenHash: newHash,
            ct: ct);

        var location = "Unknown";

        // ✅ Capture the NEW session — its Id goes into the access token
        var newSession = await _sessions.CreateAsync(
            user.Id, newHash,
            UserAgentHelper.ParseDevice(deviceInfo), browser,
            ipAddress, location,
            refreshExpiry, ct);

        _logger.LogInformation(
            "Token refreshed for UserId {UserId} from IP {Ip}",
            user.Id, ipAddress);

        var accessExpiry = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpiryMinutes);

        // ✅ newSession.Id — not activeSession.Id (that session is revoked)
        var accessToken = GenerateAccessToken(user, newSession.Id);

        return new AuthResponse(
            accessToken, newRaw, accessExpiry, refreshExpiry,
            new UserDto(user.Id, user.FullName, user.Email, user.Role.ToString()));
    }
}