using System.Security.Claims;
using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Common;
using BeautyPlusParlour.Models.DTOs.Review;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeautyPlusParlour.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class ReviewController : ControllerBase
{
    private readonly IReviewService _service;
    private readonly IValidator<CreateReviewRequest> _createV;
    private readonly IValidator<UpdateReviewRequest> _updateV;

    public ReviewController(
        IReviewService service,
        IValidator<CreateReviewRequest> createV,
        IValidator<UpdateReviewRequest> updateV)
    {
        _service = service;
        _createV = createV;
        _updateV = updateV;
    }

    // ── POST /api/reviews ─────────────────────────────────────────────────
    [HttpPost]
    [Authorize(Policy = AppRoles.CustomerOnly)]
    [ProducesResponseType(typeof(ApiResponse<ReviewDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 422)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Create(
        [FromBody] CreateReviewRequest request,
        CancellationToken ct)
    {
        var v = await _createV.ValidateAsync(request, ct);
        if (!v.IsValid)
            return UnprocessableEntity(
                ApiResponse<object>.Fail("Validation failed.",
                    v.Errors.Select(e => e.ErrorMessage)));

        var result = await _service.CreateAsync(GetUserId(), request, ct);

        return StatusCode(201,
            ApiResponse<ReviewDto>.Ok(result, ResponseMessages.ReviewCreated));
    }

    // ── GET /api/reviews/service/{serviceId} ──────────────────────────────
    [HttpGet("service/{serviceId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ReviewDto>>), 200)]
    public async Task<IActionResult> GetByService(
        Guid serviceId, CancellationToken ct)
    {
        var result = await _service.GetByServiceAsync(serviceId, ct);
        return Ok(ApiResponse<IReadOnlyList<ReviewDto>>.Ok(
            result, ResponseMessages.ReviewsFetched));
    }

    // ── GET /api/reviews/service/{serviceId}/summary ──────────────────────
    [HttpGet("service/{serviceId:guid}/summary")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ReviewSummaryDto>), 200)]
    public async Task<IActionResult> GetServiceSummary(
        Guid serviceId, CancellationToken ct)
    {
        var result = await _service.GetServiceSummaryAsync(serviceId, ct);
        return Ok(ApiResponse<ReviewSummaryDto>.Ok(result));
    }

    // ── GET /api/reviews/staff/{staffId} ──────────────────────────────────
    [HttpGet("staff/{staffId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ReviewDto>>), 200)]
    public async Task<IActionResult> GetByStaff(
        Guid staffId, CancellationToken ct)
    {
        var result = await _service.GetByStaffAsync(staffId, ct);
        return Ok(ApiResponse<IReadOnlyList<ReviewDto>>.Ok(
            result, ResponseMessages.ReviewsFetched));
    }

    // ── GET /api/reviews/staff/{staffId}/summary ──────────────────────────
    [HttpGet("staff/{staffId:guid}/summary")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ReviewSummaryDto>), 200)]
    public async Task<IActionResult> GetStaffSummary(
        Guid staffId, CancellationToken ct)
    {
        var result = await _service.GetStaffSummaryAsync(staffId, ct);
        return Ok(ApiResponse<ReviewSummaryDto>.Ok(result));
    }

    // ── GET /api/reviews/my ───────────────────────────────────────────────
    [HttpGet("my")]
    [Authorize(Policy = AppRoles.CustomerOnly)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ReviewDto>>), 200)]
    public async Task<IActionResult> GetMyReviews(CancellationToken ct)
    {
        var result = await _service.GetMyReviewsAsync(GetUserId(), ct);
        return Ok(ApiResponse<IReadOnlyList<ReviewDto>>.Ok(
            result, ResponseMessages.ReviewsFetched));
    }

    // ── GET /api/reviews/{id} ─────────────────────────────────────────────
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ReviewDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(
        Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<ReviewDto>.Ok(
            result, ResponseMessages.ReviewFetched));
    }

    // ── PUT /api/reviews/{id} ─────────────────────────────────────────────
    [HttpPut("{id:guid}")]
    [Authorize(Policy = AppRoles.CustomerOnly)]
    [ProducesResponseType(typeof(ApiResponse<ReviewDto>), 200)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateReviewRequest request,
        CancellationToken ct)
    {
        var v = await _updateV.ValidateAsync(request, ct);
        if (!v.IsValid)
            return UnprocessableEntity(
                ApiResponse<object>.Fail("Validation failed.",
                    v.Errors.Select(e => e.ErrorMessage)));

        var result = await _service.UpdateAsync(id, GetUserId(), request, ct);
        return Ok(ApiResponse<ReviewDto>.Ok(result, ResponseMessages.ReviewUpdated));
    }

    // ── PUT /api/reviews/{id}/hide ────────────────────────────────────────
    [HttpPut("{id:guid}/hide")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<ReviewDto>), 200)]
    public async Task<IActionResult> Hide(
        Guid id,
        [FromBody] HideReviewRequest request,
        CancellationToken ct)
    {
        var result = await _service.HideAsync(id, GetUserId(), request, ct);
        return Ok(ApiResponse<ReviewDto>.Ok(result, ResponseMessages.ReviewHidden));
    }

    // ── PUT /api/reviews/{id}/show ────────────────────────────────────────
    [HttpPut("{id:guid}/show")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<ReviewDto>), 200)]
    public async Task<IActionResult> Show(
        Guid id, CancellationToken ct)
    {
        var result = await _service.UnhideAsync(id, GetUserId(), ct);
        return Ok(ApiResponse<ReviewDto>.Ok(result, ResponseMessages.ReviewUnhidden));
    }

    // ── GET /api/reviews ──────────────────────────────────────────────────
    [HttpGet]
    [Authorize(Policy = AppRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ReviewDto>>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool includeHidden = false,
        CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(includeHidden, ct);
        return Ok(ApiResponse<IReadOnlyList<ReviewDto>>.Ok(
            result, ResponseMessages.ReviewsFetched));
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}