using System.Security.Claims;
using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Common;
using BeautyPlusParlour.Models.DTOs.Customer;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeautyPlusParlour.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class CustomerController : ControllerBase
{
    private readonly ICustomerService _service;
    private readonly IValidator<UpdateProfileRequest> _profileV;
    private readonly IValidator<CreateAddressRequest> _createAddrV;
    private readonly IValidator<UpdateAddressRequest> _updateAddrV;

    public CustomerController(
        ICustomerService service,
        IValidator<UpdateProfileRequest> profileV,
        IValidator<CreateAddressRequest> createAddrV,
        IValidator<UpdateAddressRequest> updateAddrV)
    {
        _service = service;
        _profileV = profileV;
        _createAddrV = createAddrV;
        _updateAddrV = updateAddrV;
    }

    // ── PROFILE ───────────────────────────────────────────────────────────

    /// <summary>Admin — list all customers</summary>
    [HttpGet]
    [Authorize(Policy = AppRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CustomerListDto>>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<CustomerListDto>>.Ok(
            result, ResponseMessages.CustomerListFetched));
    }

    /// <summary>Admin — get customer by ID</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<CustomerProfileDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(
        Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<CustomerProfileDto>.Ok(
            result, ResponseMessages.CustomerProfileFetched));
    }

    /// <summary>Customer — get own profile</summary>
    [HttpGet("my/profile")]
    [Authorize(Policy = AppRoles.CustomerOnly)]
    [ProducesResponseType(typeof(ApiResponse<CustomerProfileDto>), 200)]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
    {
        var result = await _service.GetMyProfileAsync(GetUserId(), ct);
        return Ok(ApiResponse<CustomerProfileDto>.Ok(
            result, ResponseMessages.CustomerProfileFetched));
    }

    /// <summary>Customer — update own profile</summary>
    [HttpPut("my/profile")]
    [Authorize(Policy = AppRoles.CustomerOnly)]
    [ProducesResponseType(typeof(ApiResponse<CustomerProfileDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 422)]
    public async Task<IActionResult> UpdateMyProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken ct)
    {
        var v = await _profileV.ValidateAsync(request, ct);
        if (!v.IsValid)
            return UnprocessableEntity(
                ApiResponse<object>.Fail("Validation failed.",
                    v.Errors.Select(e => e.ErrorMessage)));

        var result = await _service.UpdateMyProfileAsync(
            GetUserId(), request, ct);

        return Ok(ApiResponse<CustomerProfileDto>.Ok(
            result, ResponseMessages.CustomerProfileUpdated));
    }

    /// <summary>Admin — deactivate customer</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    public async Task<IActionResult> Deactivate(
        Guid id, CancellationToken ct)
    {
        await _service.DeactivateAsync(id, GetUserId(), ct);
        return Ok(ApiResponse<object>.Ok(
            null!, ResponseMessages.CustomerDeactivated));
    }

    // ── ADDRESSES ─────────────────────────────────────────────────────────

    /// <summary>Customer — add address</summary>
    [HttpPost("my/addresses")]
    [Authorize(Policy = AppRoles.CustomerOnly)]
    [ProducesResponseType(typeof(ApiResponse<CustomerAddressDto>), 201)]
    public async Task<IActionResult> AddAddress(
        [FromBody] CreateAddressRequest request,
        CancellationToken ct)
    {
        var v = await _createAddrV.ValidateAsync(request, ct);
        if (!v.IsValid)
            return UnprocessableEntity(
                ApiResponse<object>.Fail("Validation failed.",
                    v.Errors.Select(e => e.ErrorMessage)));

        var result = await _service.AddAddressAsync(
            GetUserId(), request, ct);

        return StatusCode(201,
            ApiResponse<CustomerAddressDto>.Ok(
                result, ResponseMessages.AddressCreated));
    }

    /// <summary>Customer — get all addresses</summary>
    [HttpGet("my/addresses")]
    [Authorize(Policy = AppRoles.CustomerOnly)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CustomerAddressDto>>), 200)]
    public async Task<IActionResult> GetAddresses(CancellationToken ct)
    {
        var result = await _service.GetAddressesAsync(GetUserId(), ct);
        return Ok(ApiResponse<IReadOnlyList<CustomerAddressDto>>.Ok(
            result, ResponseMessages.AddressListFetched));
    }

    /// <summary>Customer — get address by ID</summary>
    [HttpGet("my/addresses/{addressId:guid}")]
    [Authorize(Policy = AppRoles.CustomerOnly)]
    [ProducesResponseType(typeof(ApiResponse<CustomerAddressDto>), 200)]
    public async Task<IActionResult> GetAddress(
        Guid addressId, CancellationToken ct)
    {
        var result = await _service.GetAddressByIdAsync(
            GetUserId(), addressId, ct);

        return Ok(ApiResponse<CustomerAddressDto>.Ok(
            result, ResponseMessages.AddressFetched));
    }

    /// <summary>Customer — update address</summary>
    [HttpPut("my/addresses/{addressId:guid}")]
    [Authorize(Policy = AppRoles.CustomerOnly)]
    public async Task<IActionResult> UpdateAddress(
        Guid addressId,
        [FromBody] UpdateAddressRequest request,
        CancellationToken ct)
    {
        var v = await _updateAddrV.ValidateAsync(request, ct);
        if (!v.IsValid)
            return UnprocessableEntity(
                ApiResponse<object>.Fail("Validation failed.",
                    v.Errors.Select(e => e.ErrorMessage)));

        var result = await _service.UpdateAddressAsync(
            GetUserId(), addressId, request, ct);

        return Ok(ApiResponse<CustomerAddressDto>.Ok(
            result, ResponseMessages.AddressUpdated));
    }

    /// <summary>Customer — delete address</summary>
    [HttpDelete("my/addresses/{addressId:guid}")]
    [Authorize(Policy = AppRoles.CustomerOnly)]
    public async Task<IActionResult> DeleteAddress(
        Guid addressId, CancellationToken ct)
    {
        await _service.DeleteAddressAsync(GetUserId(), addressId, ct);
        return Ok(ApiResponse<object>.Ok(
            null!, ResponseMessages.AddressDeleted));
    }

    /// <summary>Customer — set address as default</summary>
    [HttpPut("my/addresses/{addressId:guid}/default")]
    [Authorize(Policy = AppRoles.CustomerOnly)]
    public async Task<IActionResult> SetDefault(
        Guid addressId, CancellationToken ct)
    {
        var result = await _service.SetDefaultAddressAsync(
            GetUserId(), addressId, ct);

        return Ok(ApiResponse<CustomerAddressDto>.Ok(
            result, ResponseMessages.AddressSetDefault));
    }

    // ── FAVOURITES ────────────────────────────────────────────────────────

    /// <summary>Customer — add service to favourites</summary>
    [HttpPost("my/favourites/{serviceId:guid}")]
    [Authorize(Policy = AppRoles.CustomerOnly)]
    [ProducesResponseType(typeof(ApiResponse<FavouriteServiceDto>), 200)]
    public async Task<IActionResult> AddFavourite(
        Guid serviceId, CancellationToken ct)
    {
        var result = await _service.AddFavouriteAsync(
            GetUserId(), serviceId, ct);

        return Ok(ApiResponse<FavouriteServiceDto>.Ok(
            result, ResponseMessages.FavouriteAdded));
    }

    /// <summary>Customer — get all favourites</summary>
    [HttpGet("my/favourites")]
    [Authorize(Policy = AppRoles.CustomerOnly)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FavouriteServiceDto>>), 200)]
    public async Task<IActionResult> GetFavourites(CancellationToken ct)
    {
        var result = await _service.GetFavouritesAsync(GetUserId(), ct);
        return Ok(ApiResponse<IReadOnlyList<FavouriteServiceDto>>.Ok(
            result, ResponseMessages.FavouritesFetched));
    }

    /// <summary>Customer — remove service from favourites</summary>
    [HttpDelete("my/favourites/{serviceId:guid}")]
    [Authorize(Policy = AppRoles.CustomerOnly)]
    public async Task<IActionResult> RemoveFavourite(
        Guid serviceId, CancellationToken ct)
    {
        await _service.RemoveFavouriteAsync(GetUserId(), serviceId, ct);
        return Ok(ApiResponse<object>.Ok(
            null!, ResponseMessages.FavouriteRemoved));
    }

    // ── helper ────────────────────────────────────────────────────────────
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}