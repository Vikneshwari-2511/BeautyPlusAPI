using System.Security.Claims;
using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Category;
using BeautyPlusParlour.Models.DTOs.Common;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeautyPlusParlour.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class CategoryController : ControllerBase
{
    private readonly ICategoryService _service;
    private readonly IValidator<CreateCategoryRequest> _createValidator;
    private readonly IValidator<UpdateCategoryRequest> _updateValidator;

    public CategoryController(
        ICategoryService service,
        IValidator<CreateCategoryRequest> createValidator,
        IValidator<UpdateCategoryRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // ── POST /api/categories ──────────────────────────────────────────────
    [HttpPost]
    [Authorize(Policy = AppRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryRequest request,
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
            ApiResponse<CategoryDto>.Ok(result, ResponseMessages.CategoryCreated));
    }

    // ── GET /api/categories ───────────────────────────────────────────────
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool includeInactive = false,
        CancellationToken ct = default)
    {
        // Only Admin can see inactive
        var canSeeInactive = User.Identity?.IsAuthenticated == true
            && User.IsInRole(AppRoles.Admin);

        var result = await _service.GetAllAsync(
            includeInactive && canSeeInactive, ct);

        return Ok(ApiResponse<IReadOnlyList<CategoryDto>>.Ok(result));
    }

    // ── GET /api/categories/{id} ──────────────────────────────────────────
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<CategoryDto>.Ok(result));
    }

    // ── GET /api/categories/{id}/subcategories ────────────────────────────
    [HttpGet("{id:guid}/subcategories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSubCategories(
        Guid id, CancellationToken ct)
    {
        var result = await _service.GetSubCategoriesAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    // ── PUT /api/categories/{id} ──────────────────────────────────────────
    [HttpPut("{id:guid}")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCategoryRequest request,
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

        return Ok(ApiResponse<CategoryDto>.Ok(result, ResponseMessages.CategoryUpdated));
    }

    // ── DELETE /api/categories/{id} ───────────────────────────────────────
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(
        Guid id, CancellationToken ct)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _service.DeleteAsync(id, adminId, ct);

        return Ok(ApiResponse<object>.Ok(null!, ResponseMessages.CategoryDeleted));
    }
}