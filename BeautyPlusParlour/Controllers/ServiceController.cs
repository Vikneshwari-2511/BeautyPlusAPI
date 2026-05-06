using System.Security.Claims;
using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Common;
using BeautyPlusParlour.Models.DTOs.Service;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeautyPlusParlour.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class ServiceController : ControllerBase
{
    private readonly IServiceManagementService _service;
    private readonly IValidator<CreateServiceRequest> _createValidator;
    private readonly IValidator<UpdateServiceRequest> _updateValidator;

    public ServiceController(
        IServiceManagementService service,
        IValidator<CreateServiceRequest> createValidator,
        IValidator<UpdateServiceRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // ── POST /api/services ────────────────────────────────────────────────
    [HttpPost]
    [Authorize(Policy = AppRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<ServiceDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateServiceRequest request,
        CancellationToken ct)
    {
        var validation = await _createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return UnprocessableEntity(
                ApiResponse<object>.Fail(
                    "Validation failed.",
                    validation.Errors.Select(e => e.ErrorMessage)));

        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _service.CreateAsync(request, adminId, ct);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<ServiceDto>.Ok(result, ResponseMessages.ServiceCreated));
    }

    // ── GET /api/services ─────────────────────────────────────────────────
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<ServiceListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] ServiceFilterRequest filter,
        CancellationToken ct)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return Ok(ApiResponse<PagedResponse<ServiceListDto>>.Ok(
            result, ResponseMessages.ServicesFetched));
    }

    // ── GET /api/services/featured ────────────────────────────────────────
    [HttpGet("featured")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFeatured(CancellationToken ct)
    {
        var result = await _service.GetFeaturedAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<ServiceListDto>>.Ok(result));
    }

    // ── GET /api/services/{id} ────────────────────────────────────────────
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ServiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<ServiceDto>.Ok(result));
    }

    // ── PUT /api/services/{id} ────────────────────────────────────────────
    [HttpPut("{id:guid}")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateServiceRequest request,
        CancellationToken ct)
    {
        var validation = await _updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return UnprocessableEntity(
                ApiResponse<object>.Fail(
                    "Validation failed.",
                    validation.Errors.Select(e => e.ErrorMessage)));

        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _service.UpdateAsync(id, request, adminId, ct);

        return Ok(ApiResponse<ServiceDto>.Ok(result, ResponseMessages.ServiceUpdated));
    }

    // ── PUT /api/services/{id}/toggle-active ──────────────────────────────
    [HttpPut("{id:guid}/toggle-active")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    public async Task<IActionResult> ToggleActive(Guid id, CancellationToken ct)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _service.ToggleActiveAsync(id, adminId, ct);

        return Ok(ApiResponse<object>.Ok(null!, ResponseMessages.ServiceToggled));
    }

    // ── DELETE /api/services/{id} ─────────────────────────────────────────
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _service.DeleteAsync(id, adminId, ct);

        return Ok(ApiResponse<object>.Ok(null!, ResponseMessages.ServiceDeleted));
    }
}