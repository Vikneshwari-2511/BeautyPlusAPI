using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Data;
using BeautyPlusParlour.Exceptions;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Auth;
using BeautyPlusParlour.Models.Entities;
using BeautyPlusParlour.Models.Enums;
using FirebaseAdmin.Auth;
using Microsoft.EntityFrameworkCore;

namespace BeautyPlusParlour.Services;

public sealed class FirebaseAuthService : IFirebaseAuthService
{
    private readonly AppDbContext _db;
    private readonly ITokenService _token;
    private readonly ILogger<FirebaseAuthService> _logger;

    public FirebaseAuthService(
        AppDbContext db,
        ITokenService token,
        ILogger<FirebaseAuthService> logger)
    {
        _db = db;
        _token = token;
        _logger = logger;
    }

    public async Task<AuthResponse> VerifyAndLoginAsync(
        FirebasePhoneLoginRequest request,
        string ipAddress,
        string userAgent,
        CancellationToken ct = default)
    {
        // ── Step 1: Verify Firebase token ────────────────
        FirebaseToken decoded;
        try
        {
            decoded = await FirebaseAuth.DefaultInstance
                .VerifyIdTokenAsync(request.FirebaseToken, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                "Firebase token verification failed: {Msg}",
                ex.Message);
            throw new UnauthorizedException(
                ResponseMessages.FirebaseTokenInvalid);
        }

        // ── Step 2: Extract phone ─────────────────────────
        var phone = decoded.Claims
            .GetValueOrDefault("phone_number")
            ?.ToString()
            ?? request.PhoneNumber;

        // ── Step 3: Find or create user ───────────────────
        var user = await _db.Users
            .FirstOrDefaultAsync(u =>
                u.PhoneNumber == phone, ct);

        if (user is null)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(
                Guid.NewGuid().ToString(),
                workFactor: 12);

            // User.Create(fullName, email, passwordHash, phoneNumber, role)
            user = User.Create(
                fullName: $"Customer {phone[^4..]}",
                email: $"phone_{phone.Replace("+", "")}@beautyplus.temp",
                passwordHash: passwordHash,
                phoneNumber: phone,
                role: UserRole.Customer);

            user.VerifyEmail();

            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "New user created via phone: {Phone}", phone);
        }

        if (!user.IsActive)
            throw new UnauthorizedException(
                "Your account has been deactivated.");

        // ── Step 4: Generate tokens ───────────────────────
        var sessionId = Guid.NewGuid();

        // GenerateAccessToken(User user, Guid sessionId)
        var accessToken = _token.GenerateAccessToken(user, sessionId);

        // GenerateRefreshToken() → (rawToken, tokenHash)
        var (rawToken, tokenHash) = _token.GenerateRefreshToken();

        // ── Step 5: Create UserSession ────────────────────
        // UserSession.Create(userId, refreshTokenHash, deviceInfo,
        //                    browser, ipAddress, location, expiresAt)
        var session = UserSession.Create(
            userId: user.Id,
            refreshTokenHash: tokenHash,
            deviceInfo: userAgent,
            browser: ParseBrowser(userAgent),
            ipAddress: ipAddress,
            location: "Unknown",
            expiresAt: DateTime.UtcNow.AddDays(7));

        _db.UserSessions.Add(session);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Firebase phone login success. UserId: {Id}",
            user.Id);

        // ── Step 6: Return AuthResponse ───────────────────
        // AuthResponse is a record: (AccessToken, RefreshToken,
        //   AccessTokenExpiresAt, RefreshTokenExpiresAt, User)
        // UserDto is a record: (Id, FullName, Email, Role)
        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: rawToken,
            AccessTokenExpiresAt: DateTime.UtcNow.AddMinutes(15),
            RefreshTokenExpiresAt: DateTime.UtcNow.AddDays(7),
            User: new UserDto(
                Id: user.Id,
                FullName: user.FullName,
                Email: user.Email,
                Role: user.Role.ToString()
            )
        );
    }

    // ── private ───────────────────────────────────────────
    private static string ParseBrowser(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
            return "Unknown";
        if (userAgent.Contains("Chrome")) return "Chrome";
        if (userAgent.Contains("Firefox")) return "Firefox";
        if (userAgent.Contains("Safari")) return "Safari";
        if (userAgent.Contains("Edge")) return "Edge";
        return "Other";
    }
}