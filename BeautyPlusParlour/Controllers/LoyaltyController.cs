using System.Security.Claims;
using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Common;
using BeautyPlusParlour.Models.DTOs.Loyalty;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeautyPlusParlour.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class LoyaltyController : ControllerBase
{
    private readonly ILoyaltyService _loyalty;
    private readonly IValidator<AdjustPointsRequest> _adjustV;
    private readonly IValidator<ValidateRedeemRequest> _redeemV;

    public LoyaltyController(
        ILoyaltyService loyalty,
        IValidator<AdjustPointsRequest> adjustV,
        IValidator<ValidateRedeemRequest> redeemV)
    {
        _loyalty = loyalty;
        _adjustV = adjustV;
        _redeemV = redeemV;
    }

    // ── GET /api/loyalty/my-points ────────────────────────────────────────
    [HttpGet("my-points")]
    [Authorize(Policy = AppRoles.CustomerOnly)]
    [ProducesResponseType(typeof(ApiResponse<LoyaltyPointsDto>), 200)]
    public async Task<IActionResult> GetMyPoints(CancellationToken ct)
    {
        var result = await _loyalty.GetMyPointsAsync(GetUserId(), ct);
        return Ok(ApiResponse<LoyaltyPointsDto>.Ok(
            result, ResponseMessages.LoyaltyPointsFetched));
    }

    // ── GET /api/loyalty/my-transactions ─────────────────────────────────
    [HttpGet("my-transactions")]
    [Authorize(Policy = AppRoles.CustomerOnly)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<LoyaltyTransactionDto>>), 200)]
    public async Task<IActionResult> GetMyTransactions(CancellationToken ct)
    {
        var result = await _loyalty.GetMyTransactionsAsync(GetUserId(), ct);
        return Ok(ApiResponse<IReadOnlyList<LoyaltyTransactionDto>>.Ok(
            result, ResponseMessages.LoyaltyTransactionsFetched));
    }

    // ── POST /api/loyalty/validate-redeem ─────────────────────────────────
    [HttpPost("validate-redeem")]
    [Authorize(Policy = AppRoles.CustomerOnly)]
    [ProducesResponseType(typeof(ApiResponse<ValidateRedeemResponse>), 200)]
    public async Task<IActionResult> ValidateRedeem(
        [FromBody] ValidateRedeemRequest request,
        CancellationToken ct)
    {
        var v = await _redeemV.ValidateAsync(request, ct);
        if (!v.IsValid)
            return UnprocessableEntity(
                ApiResponse<object>.Fail("Validation failed.",
                    v.Errors.Select(e => e.ErrorMessage)));

        var result = await _loyalty.ValidateRedeemAsync(GetUserId(), request, ct);
        return Ok(ApiResponse<ValidateRedeemResponse>.Ok(
            result, ResponseMessages.ValidateRedeemSuccess));
    }

    // ── POST /api/loyalty/adjust (Admin) ──────────────────────────────────
    [HttpPost("adjust")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<LoyaltyPointsDto>), 200)]
    public async Task<IActionResult> Adjust(
        [FromBody] AdjustPointsRequest request,
        CancellationToken ct)
    {
        var v = await _adjustV.ValidateAsync(request, ct);
        if (!v.IsValid)
            return UnprocessableEntity(
                ApiResponse<object>.Fail("Validation failed.",
                    v.Errors.Select(e => e.ErrorMessage)));

        var result = await _loyalty.AdjustAsync(request, GetUserId(), ct);
        return Ok(ApiResponse<LoyaltyPointsDto>.Ok(
            result, ResponseMessages.LoyaltyAdjusted));
    }

    // ── GET /api/loyalty/customers (Admin) ────────────────────────────────
    [HttpGet("customers")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<LoyaltyPointsDto>>), 200)]
    public async Task<IActionResult> GetAllCustomers(CancellationToken ct)
    {
        var result = await _loyalty.GetAllCustomersAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<LoyaltyPointsDto>>.Ok(
            result, ResponseMessages.LoyaltyCustomersFetched));
    }

    // ── GET /api/loyalty/customers/{id} (Admin) ───────────────────────────
    [HttpGet("customers/{customerId:guid}")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<LoyaltyPointsDto>), 200)]
    public async Task<IActionResult> GetCustomerPoints(
        Guid customerId, CancellationToken ct)
    {
        var result = await _loyalty.GetByCustomerIdAsync(customerId, ct);
        return Ok(ApiResponse<LoyaltyPointsDto>.Ok(
            result, ResponseMessages.LoyaltyPointsFetched));
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}