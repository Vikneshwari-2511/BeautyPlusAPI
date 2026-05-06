using System.Security.Claims;
using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Common;
using BeautyPlusParlour.Models.DTOs.Coupon;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeautyPlusParlour.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class CouponController : ControllerBase
{
    private readonly ICouponService _service;
    private readonly IValidator<CreateCouponRequest> _createV;
    private readonly IValidator<UpdateCouponRequest> _updateV;
    private readonly IValidator<ValidateCouponRequest> _validateV;

    public CouponController(
        ICouponService service,
        IValidator<CreateCouponRequest> createV,
        IValidator<UpdateCouponRequest> updateV,
        IValidator<ValidateCouponRequest> validateV)
    {
        _service = service;
        _createV = createV;
        _updateV = updateV;
        _validateV = validateV;
    }

    // ── POST /api/coupons ─────────────────────────────────────────────────
    [HttpPost]
    [Authorize(Policy = AppRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<CouponDto>), 201)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCouponRequest request,
        CancellationToken ct)
    {
        var v = await _createV.ValidateAsync(request, ct);
        if (!v.IsValid)
            return UnprocessableEntity(
                ApiResponse<object>.Fail("Validation failed.",
                    v.Errors.Select(e => e.ErrorMessage)));

        var result = await _service.CreateAsync(request, GetUserId(), ct);
        return StatusCode(201,
            ApiResponse<CouponDto>.Ok(result, ResponseMessages.CouponCreated));
    }

    // ── GET /api/coupons ──────────────────────────────────────────────────
    [HttpGet]
    [Authorize(Policy = AppRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CouponDto>>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool includeInactive = false,
        CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(includeInactive, ct);
        return Ok(ApiResponse<IReadOnlyList<CouponDto>>.Ok(
            result, ResponseMessages.CouponsFetched));
    }

    // ── GET /api/coupons/{id} ─────────────────────────────────────────────
    [HttpGet("{id:guid}")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<CouponDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(
        Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<CouponDto>.Ok(result));
    }

    // ── PUT /api/coupons/{id} ─────────────────────────────────────────────
    [HttpPut("{id:guid}")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCouponRequest request,
        CancellationToken ct)
    {
        var v = await _updateV.ValidateAsync(request, ct);
        if (!v.IsValid)
            return UnprocessableEntity(
                ApiResponse<object>.Fail("Validation failed.",
                    v.Errors.Select(e => e.ErrorMessage)));

        var result = await _service.UpdateAsync(id, request, GetUserId(), ct);
        return Ok(ApiResponse<CouponDto>.Ok(result, ResponseMessages.CouponUpdated));
    }

    // ── DELETE /api/coupons/{id} ──────────────────────────────────────────
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    public async Task<IActionResult> Deactivate(
        Guid id, CancellationToken ct)
    {
        await _service.DeactivateAsync(id, GetUserId(), ct);
        return Ok(ApiResponse<object>.Ok(
            null!, ResponseMessages.CouponDeactivated));
    }

    // ── POST /api/coupons/validate ────────────────────────────────────────
    [HttpPost("validate")]
    [Authorize(Policy = AppRoles.CustomerOnly)]
    [ProducesResponseType(typeof(ApiResponse<ValidateCouponResponse>), 200)]
    public async Task<IActionResult> Validate(
        [FromBody] ValidateCouponRequest request,
        CancellationToken ct)
    {
        var v = await _validateV.ValidateAsync(request, ct);
        if (!v.IsValid)
            return UnprocessableEntity(
                ApiResponse<object>.Fail("Validation failed.",
                    v.Errors.Select(e => e.ErrorMessage)));

        var result = await _service.ValidateAsync(GetUserId(), request, ct);
        return Ok(ApiResponse<ValidateCouponResponse>.Ok(result));
    }

    // ── GET /api/coupons/my-usage ─────────────────────────────────────────
    [HttpGet("my-usage")]
    [Authorize(Policy = AppRoles.CustomerOnly)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CouponUsageDto>>), 200)]
    public async Task<IActionResult> GetMyUsage(CancellationToken ct)
    {
        var result = await _service.GetMyUsageAsync(GetUserId(), ct);
        return Ok(ApiResponse<IReadOnlyList<CouponUsageDto>>.Ok(
            result, ResponseMessages.CouponUsageFetched));
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}