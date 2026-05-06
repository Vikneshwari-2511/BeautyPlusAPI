using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Data;
using BeautyPlusParlour.Exceptions;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Auth;
using BeautyPlusParlour.Models.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace BeautyPlusParlour.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class PasswordController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IOtpService _otps;
    private readonly IEmailService _emails;
    private readonly ISessionService _sessions;

    public PasswordController(
        AppDbContext db,
        IOtpService otps,
        IEmailService emails,
        ISessionService sessions)
    {
        _db = db;
        _otps = otps;
        _emails = emails;
        _sessions = sessions;
    }

    // ── POST /api/password/forgot-password ────────────────────────────────
    /// <summary>
    /// Sends a password reset OTP to the user's email.
    /// Always returns 200 to prevent email enumeration.
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("otp")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            !request.Email.Contains('@'))
            return BadRequest(ApiResponse<object>.Fail("A valid email is required."));

        var user = await _db.Users
            .FirstOrDefaultAsync(
                u => u.Email == request.Email.ToLowerInvariant()
                  && u.IsActive, ct);

        if (user is not null)
        {
            var otp = await _otps.GenerateAsync(
                user.Id, OtpPurpose.PasswordReset, ct);

            await _emails.SendOtpEmailAsync(
                user.Email, user.FullName, otp, ct);
        }

        // Always return same response — prevents knowing if email exists
        return Ok(ApiResponse<object>.Ok(null!, ResponseMessages.OtpSent));
    }

    // ── POST /api/password/reset-password ─────────────────────────────────
    /// <summary>
    /// Verifies the OTP and resets the user's password.
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("otp")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        // ── Validation ────────────────────────────────────────────────────
        if (string.IsNullOrWhiteSpace(request.Email) ||
            !request.Email.Contains('@'))
            return BadRequest(ApiResponse<object>.Fail("A valid email is required."));

        if (string.IsNullOrWhiteSpace(request.Otp))
            return BadRequest(ApiResponse<object>.Fail("OTP is required."));

        if (string.IsNullOrWhiteSpace(request.NewPassword) ||
            request.NewPassword.Length < 8)
            return BadRequest(ApiResponse<object>.Fail(
                "Password must be at least 8 characters."));

        if (request.NewPassword != request.ConfirmNewPassword)
            return BadRequest(ApiResponse<object>.Fail(ResponseMessages.PasswordMismatch));

        // ── Lookup user ───────────────────────────────────────────────────
        var user = await _db.Users
            .FirstOrDefaultAsync(
                u => u.Email == request.Email.ToLowerInvariant()
                  && u.IsActive, ct)
            ?? throw new NotFoundException("No active account found for this email.");

        // ── Verify OTP ────────────────────────────────────────────────────
        var isValid = await _otps.VerifyAsync(
            user.Id, request.Otp, OtpPurpose.PasswordReset, ct);

        if (!isValid)
            return BadRequest(ApiResponse<object>.Fail(ResponseMessages.InvalidOtp));

        // ── Update password ───────────────────────────────────────────────
        var newHash = BCrypt.Net.BCrypt.HashPassword(
            request.NewPassword, workFactor: 12);

        user.UpdatePassword(newHash);
        // revoke all sessions so old refresh tokens are dead
        await _sessions.RevokeAllAsync(user.Id, ct);

        await _db.SaveChangesAsync(ct);

        await _emails.SendPasswordChangedEmailAsync(
            user.Email, user.FullName, ct);

        return Ok(ApiResponse<object>.Ok(null!, ResponseMessages.PasswordResetSuccess));
    }
}