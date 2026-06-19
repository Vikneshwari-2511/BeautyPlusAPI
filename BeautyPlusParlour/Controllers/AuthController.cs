using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Data;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Auth;
using BeautyPlusParlour.Models.DTOs.Common;
using BeautyPlusParlour.Models.Entities;
using BeautyPlusParlour.Models.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BeautyPlusParlour.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly ISessionService _sessions;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<ResendVerificationRequest> _resendValidator;
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IFirebaseAuthService _firebaseAuth;
    public AuthController(
        IAuthService auth,
        IFirebaseAuthService firebaseAuth,
        ISessionService sessions,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        IValidator<ResendVerificationRequest> resendValidator,
        AppDbContext db,
        IWebHostEnvironment env)
    {
        _auth = auth;
        _firebaseAuth = firebaseAuth;
        _sessions = sessions;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _resendValidator = resendValidator;
        _db = db;
        _env = env;
    }

    // ── POST /api/auth/register ───────────────────────────────────────────
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request, CancellationToken ct)
    {
        var validation = await _registerValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return UnprocessableEntity(
                ApiResponse<object>.Fail(
                    "Validation failed.",
                    validation.Errors.Select(e => e.ErrorMessage)));

        var response = await _auth.RegisterAsync(request, ct);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<AuthResponse>.Ok(response, ResponseMessages.RegisterSuccess));
    }

    // ── POST /api/auth/login ──────────────────────────────────────────────
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request, CancellationToken ct)
    {
        var validation = await _loginValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return UnprocessableEntity(
                ApiResponse<object>.Fail(
                    "Validation failed.",
                    validation.Errors.Select(e => e.ErrorMessage)));

        var deviceInfo = Request.Headers.UserAgent.ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        var response = await _auth.LoginAsync(request, deviceInfo, ipAddress, ct);

        return Ok(ApiResponse<AuthResponse>.Ok(response, ResponseMessages.LoginSuccess));
    }

    // ── POST /api/auth/logout ─────────────────────────────────────────────────
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(
        [FromQuery] Guid sessionId, CancellationToken ct)
    {
        await _auth.LogoutAsync(sessionId, ct);
        return Ok(ApiResponse<object>.Ok(null!, ResponseMessages.LogoutSuccess));
    }

    // ── POST /api/auth/logout-all ─────────────────────────────────────────
    [HttpPost("logout-all")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogoutAll(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _auth.LogoutAllDevicesAsync(userId, ct);
        return Ok(ApiResponse<object>.Ok(null!, ResponseMessages.LogoutAllSuccess));
    }
    // GET /api/auth/check-email?email=test@gmail.com
    [HttpGet("check-email")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckEmail(
        [FromQuery] string email,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { exists = false });

        var exists = await _db.Users
            .AnyAsync(u => u.Email == email.ToLower().Trim(), ct);

        // Always return 200 (don't reveal user existence via status code)
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Email check completed.",
            Data = new { exists }
        });
    }
    // ── GET /api/auth/verify-email?token=... ─────────────────────────────
    [HttpGet("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail(
        [FromQuery] string token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest(ApiResponse<object>.Fail("Token is required."));

        await _auth.VerifyEmailAsync(token, ct);
        return Ok(ApiResponse<object>.Ok(null!, ResponseMessages.EmailVerified));
    }

    // ── POST /api/auth/resend-verification ───────────────────────────────
    [HttpPost("resend-verification")]
    [AllowAnonymous]
    [EnableRateLimiting("resend")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendVerification(
        [FromBody] ResendVerificationRequest request, CancellationToken ct)
    {
        var validation = await _resendValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return UnprocessableEntity(
                ApiResponse<object>.Fail(
                    "Validation failed.",
                    validation.Errors.Select(e => e.ErrorMessage)));

        await _auth.ResendVerificationEmailAsync(request.Email, ct);
        return Ok(ApiResponse<object>.Ok(null!, ResponseMessages.VerificationResent));
    }

    [HttpGet("sessions")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SessionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSessions(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var sidClaim = User.FindFirstValue("sid");
        var currentSessionId = sidClaim is not null
            ? Guid.Parse(sidClaim)
            : (Guid?)null;

        var sessions = await _sessions.GetActiveSessionsAsync(
            userId,
            currentSessionId,
            ct);

        return Ok(ApiResponse<IReadOnlyList<SessionDto>>.Ok(
            sessions, ResponseMessages.SessionsFetched));
    }
    // GET /api/auth/google-login
    [HttpGet("google-login")]
    [AllowAnonymous]
    public IActionResult GoogleLogin()
    {
        var redirectUrl = Url.Action("GoogleCallback", "Auth");
        var properties = new AuthenticationProperties
        {
            RedirectUri = redirectUrl
        };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }
    // ── POST /api/auth/google-login ───────────────────────────────────────────
    [HttpPost("google-login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GoogleLogin(
        [FromBody] GoogleLoginRequest request, CancellationToken ct)
    {
        var response = await _auth.GoogleLoginAsync(request, ct);
        return Ok(ApiResponse<AuthResponse>.Ok(
            response, "Google login successful."));
    }
    
    // ADD endpoint
    [HttpPost("phone-login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
    public async Task<IActionResult> PhoneLogin(
        [FromBody] FirebasePhoneLoginRequest request,
        CancellationToken ct)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?
            .ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();

        var response = await _firebaseAuth.VerifyAndLoginAsync(
            request, ipAddress, userAgent, ct);

        return Ok(ApiResponse<AuthResponse>.Ok(
            response, ResponseMessages.FirebaseLoginSuccess));
    }
}