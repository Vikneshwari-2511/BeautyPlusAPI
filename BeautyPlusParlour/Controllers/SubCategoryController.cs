using System.Security.Claims;
using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Common;
using BeautyPlusParlour.Models.DTOs.SubCategory;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeautyPlusParlour.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class SubCategoryController : ControllerBase
{
    private readonly ISubCategoryService _service;
    private readonly IValidator<CreateSubCategoryRequest> _createValidator;
    private readonly IValidator<UpdateSubCategoryRequest> _updateValidator;

    public SubCategoryController(
        ISubCategoryService service,
        IValidator<CreateSubCategoryRequest> createValidator,
        IValidator<UpdateSubCategoryRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpPost]
    [Authorize(Policy = AppRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<SubCategoryDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSubCategoryRequest request,
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
            ApiResponse<SubCategoryDto>.Ok(result, ResponseMessages.SubCategoryCreated));
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<SubCategoryDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<SubCategoryDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSubCategoryRequest request,
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

        return Ok(ApiResponse<SubCategoryDto>.Ok(result, ResponseMessages.SubCategoryUpdated));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _service.DeleteAsync(id, adminId, ct);

        return Ok(ApiResponse<object>.Ok(null!, ResponseMessages.SubCategoryDeleted));
    }
}