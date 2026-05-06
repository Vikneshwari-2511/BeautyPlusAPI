using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Helpers;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Auth;
using BeautyPlusParlour.Models.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BeautyPlusParlour.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class TokenController : ControllerBase
{
    private readonly ITokenService _tokens;
    private readonly ISessionService _sessions;

    public TokenController(ITokenService tokens, ISessionService sessions)
    {
        _tokens = tokens;
        _sessions = sessions;
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest(ApiResponse<object>.Fail("Refresh token is required."));

        var deviceInfo = Request.Headers.UserAgent.ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        var response = await _tokens.RefreshAsync(
            request.RefreshToken, deviceInfo, ipAddress, ct);

        return Ok(ApiResponse<AuthResponse>.Ok(response, ResponseMessages.TokenRefreshed));
    }

    // ── POST /api/token/revoke ────────────────────────────────────────────────
    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke(
        [FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest(ApiResponse<object>.Fail("Refresh token is required."));

        var hash = HashHelper.ToSha256Base64(request.RefreshToken);
        var session = await _sessions.GetActiveByTokenHashAsync(hash, ct);

        if (session is not null)
            await _sessions.RevokeAsync(
                sessionId: session.Id,
                replacedByTokenHash: null,
                ct: ct);

        return Ok(ApiResponse<object>.Ok(null!, ResponseMessages.TokenRevoked));
    }
}