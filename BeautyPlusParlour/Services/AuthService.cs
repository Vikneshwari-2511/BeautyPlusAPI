using BeautyPlusParlour.Configurations;
using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Data;
using BeautyPlusParlour.Exceptions;
using BeautyPlusParlour.Helpers;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Auth;
using BeautyPlusParlour.Models.Entities;
using BeautyPlusParlour.Models.Enums;
using FirebaseAdmin.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace BeautyPlusParlour.Services;

public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokens;
    private readonly ISessionService _sessions;
    private readonly IOtpService _otps;
    private readonly IEmailService _emails;
    private readonly JwtSettings _jwt;
    private readonly string _baseUrl;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext db,
        ITokenService tokens,
        ISessionService sessions,
        IOtpService otps,
        IEmailService emails,
        IOptions<JwtSettings> jwt,
        IConfiguration config,
        ILogger<AuthService> logger)
    {
        _db = db;
        _tokens = tokens;
        _sessions = sessions;
        _otps = otps;
        _emails = emails;
        _jwt = jwt.Value;
        _baseUrl = config["App:BaseUrl"]!;
        _logger = logger;
    }

    // ── Register ──────────────────────────────────────────────────────────
    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request, CancellationToken ct = default)
    {
        var emailExists = await _db.Users
            .AnyAsync(u => u.Email == request.Email.ToLowerInvariant(), ct);

        if (emailExists)
            throw new AppException(ResponseMessages.EmailAlreadyExists);

        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

        var user = User.Create(
            request.FullName, request.Email,
            hash, request.PhoneNumber, request.Role);

        _db.Users.Add(user);

        var verifyToken = GenerateSecureToken();
        _db.EmailVerificationTokens.Add(
            EmailVerificationToken.Create(user.Id, verifyToken));

        await _db.SaveChangesAsync(ct);

        var link = BuildVerificationLink(verifyToken);
        await _emails.SendVerificationEmailAsync(
            user.Email, user.FullName, link, ct);

        _logger.LogInformation(
            "New user registered: {Email}, Role: {Role}",
            user.Email, user.Role);

        return await BuildAuthResponseAsync(user, "Registration", "Registration", ct);
    }
  
    // ── Google Login ──────────────────────────────────────────────────────────
    public async Task<AuthResponse>
GoogleLoginAsync(

 GoogleLoginRequest request,
 CancellationToken ct = default)

    {

        var payload =
          await FirebaseAuth
             .DefaultInstance
             .VerifyIdTokenAsync(
                  request.IdToken
             );

        var email =
           payload.Claims["email"]
                  ?.ToString();

        var name =
           payload.Claims["name"]
                  ?.ToString();

        if (string.IsNullOrEmpty(email))
            throw new AppException(
              "Invalid token"
            );


        var user =
        await _db.Users
             .FirstOrDefaultAsync(

             u => u.Email ==
                 email.ToLower(),

             ct);


        if (user is null)
        {

            user =
               User.Create(

                 name ?? "User",

                 email,

                 GenerateSecureToken(),

                 string.Empty,

                 UserRole.Customer

               );

            _db.Users.Add(user);

            await _db.SaveChangesAsync(
               ct
            );

        }


        _logger.LogInformation(

          "Firebase login: {Email}",

          email

        );


        return await
        BuildAuthResponseAsync(

          user,

          "GoogleLogin",

          "GoogleLogin",

          ct

        );

    }
    // ── Login ─────────────────────────────────────────────────────────────
    public async Task<AuthResponse> LoginAsync(
        LoginRequest request, string deviceInfo,
        string ipAddress, CancellationToken ct = default)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(
                u => u.Email == request.Email.ToLowerInvariant()
                  && u.IsActive, ct)
            ?? throw new UnauthorizedException(ResponseMessages.InvalidCredentials);

        if (user.IsLockedOut())
            throw new UnauthorizedException(
                "Account temporarily locked due to multiple failed attempts. Try again later.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            await _db.SaveChangesAsync(ct);

            _logger.LogWarning(
                "Failed login for {Email} from IP {Ip}. Attempt #{Count}",
                user.Email, ipAddress, user.FailedLoginCount);

            throw new UnauthorizedException(ResponseMessages.InvalidCredentials);
        }

        if (!user.IsEmailVerified)
            throw new AppException(ResponseMessages.EmailNotVerified);

        user.ResetFailedLogin();
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Successful login for {Email} from IP {Ip}",
            user.Email, ipAddress);

        return await BuildAuthResponseAsync(user, deviceInfo, ipAddress, ct);
    }
    
    public async Task LogoutAsync(Guid sessionId, CancellationToken ct = default) =>
    await _sessions.RevokeAsync(
        sessionId: sessionId,
        replacedByTokenHash: null,
        ct: ct);
    public async Task LogoutAllDevicesAsync(Guid userId, CancellationToken ct = default) =>
        await _sessions.RevokeAllAsync(userId, ct);

    // ── Verify Email ──────────────────────────────────────────────────────
    public async Task VerifyEmailAsync(string token, CancellationToken ct = default)
    {
        var record = await _db.EmailVerificationTokens
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Token == token && !e.IsUsed, ct)
            ?? throw new AppException(ResponseMessages.InvalidToken);

        if (record.IsExpired())
            throw new AppException(
                "Verification link has expired. Please request a new one.");

        record.MarkUsed();
        record.User.VerifyEmail();
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Email verified for UserId {UserId}", record.UserId);
    }

    // ── Resend Verification ───────────────────────────────────────────────
    public async Task ResendVerificationEmailAsync(
        string email, CancellationToken ct = default)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(
                u => u.Email == email.ToLowerInvariant() && u.IsActive, ct)
            ?? throw new NotFoundException("User not found.");

        if (user.IsEmailVerified)
            throw new AppException(ResponseMessages.EmailAlreadyVerified);

        var existingTokens = await _db.EmailVerificationTokens
            .Where(e => e.UserId == user.Id && !e.IsUsed)
            .ToListAsync(ct);

        foreach (var old in existingTokens)
            old.MarkUsed();

        var newToken = GenerateSecureToken();
        _db.EmailVerificationTokens.Add(
            EmailVerificationToken.Create(user.Id, newToken));

        await _db.SaveChangesAsync(ct);

        var link = BuildVerificationLink(newToken);
        await _emails.SendVerificationEmailAsync(
            user.Email, user.FullName, link, ct);

        _logger.LogInformation(
            "Verification email resent for {Email}", email);
    }

    // ── private helpers ───────────────────────────────────────────────────
    public async Task<AuthResponse> BuildAuthResponseAsync(
    User user, string deviceInfo,
    string ipAddress, CancellationToken ct)
    {
        var (rawRefresh, refreshHash) = _tokens.GenerateRefreshToken();
        var refreshExpiry = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays);

        var browser = UserAgentHelper.ParseBrowser(deviceInfo);
        var device = UserAgentHelper.ParseDevice(deviceInfo);
        var location = "Unknown";

        var session = await _sessions.CreateAsync(      // ← capture returned session
            userId: user.Id,
            tokenHash: refreshHash,
            deviceInfo: device,
            browser: browser,
            ipAddress: ipAddress,
            location: location,
            expiresAt: refreshExpiry,
            ct: ct);

        var accessToken = _tokens.GenerateAccessToken(user, session.Id);  // ← pass sessionId
        var accessExpiry = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpiryMinutes);

        return new AuthResponse(
            accessToken, rawRefresh, accessExpiry, refreshExpiry,
            new UserDto(user.Id, user.FullName, user.Email, user.Role.ToString()));
    }

    private static string GenerateSecureToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    private string BuildVerificationLink(string token) =>
        $"{_baseUrl}/api/auth/verify-email?token={Uri.EscapeDataString(token)}";
}