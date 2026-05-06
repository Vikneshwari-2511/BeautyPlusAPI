using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Data;
using BeautyPlusParlour.Exceptions;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Customer;
using BeautyPlusParlour.Models.Entities;
using BeautyPlusParlour.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BeautyPlusParlour.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        AppDbContext db,
        IAuditService audit,
        ILogger<CustomerService> logger)
    {
        _db = db;
        _audit = audit;
        _logger = logger;
    }

    // ── Profile ───────────────────────────────────────────────────────────

    public async Task<CustomerProfileDto> GetMyProfileAsync(
        Guid userId, CancellationToken ct = default)
    {
        var profile = await GetOrCreateProfileAsync(userId, ct);
        return MapToDto(profile);
    }

    public async Task<CustomerProfileDto> UpdateMyProfileAsync(
        Guid userId, UpdateProfileRequest request,
        CancellationToken ct = default)
    {
        var profile = await GetOrCreateProfileAsync(userId, ct);

        profile.Update(
            request.FullName, request.PhoneNumber,
            request.DateOfBirth, request.Gender,
            request.ProfileImageUrl);

        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            userId, AuditAction.Updated,
            "CustomerProfile", profile.Id.ToString(),
            ct: ct);

        return MapToDto(profile);
    }

    public async Task<CustomerProfileDto> GetByIdAsync(
        Guid customerId, CancellationToken ct = default)
    {
        var profile = await _db.CustomerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == customerId, ct)
            ?? throw new NotFoundException(ResponseMessages.CustomerNotFound);

        return MapToDto(profile);
    }

    public async Task<IReadOnlyList<CustomerListDto>> GetAllAsync(
        CancellationToken ct = default)
    {
        var profiles = await _db.CustomerProfiles
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);

        var result = new List<CustomerListDto>();

        foreach (var p in profiles)
        {
            // Booking count will come from Module 5
            // For now return 0 as placeholder
            result.Add(new CustomerListDto(
                p.Id, p.FullName, p.PhoneNumber,
                p.Gender, p.IsActive, p.CreatedAt,
                TotalBookings: 0));
        }

        return result.AsReadOnly();
    }

    public async Task DeactivateAsync(
        Guid customerId, Guid adminId,
        CancellationToken ct = default)
    {
        var profile = await _db.CustomerProfiles
            .FirstOrDefaultAsync(c => c.Id == customerId, ct)
            ?? throw new NotFoundException(ResponseMessages.CustomerNotFound);

        profile.Deactivate();
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            adminId, AuditAction.Deleted,
            "CustomerProfile", customerId.ToString(),
            ct: ct);
    }

    // ── Addresses ─────────────────────────────────────────────────────────

    public async Task<CustomerAddressDto> AddAddressAsync(
        Guid userId, CreateAddressRequest request,
        CancellationToken ct = default)
    {
        var profile = await GetOrCreateProfileAsync(userId, ct);

        var count = await _db.CustomerAddresses
            .CountAsync(a => a.CustomerId == profile.Id && a.IsActive, ct);

        if (count >= CustomerConstants.MaxAddressesPerCustomer)
            throw new AppException(ResponseMessages.AddressLimitReached);

        // First address → auto default
        var isDefault = count == 0;

        var address = CustomerAddress.Create(
            profile.Id, request.Label,
            request.AddressLine1, request.AddressLine2,
            request.City, request.State,
            request.PinCode, request.Landmark,
            isDefault);

        _db.CustomerAddresses.Add(address);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Address added for CustomerId {Id}: {Label}",
            profile.Id, request.Label);

        return MapAddressToDto(address);
    }

    public async Task<CustomerAddressDto> UpdateAddressAsync(
        Guid userId, Guid addressId,
        UpdateAddressRequest request,
        CancellationToken ct = default)
    {
        var address = await GetAddressForUserAsync(userId, addressId, ct);

        address.Update(
            request.Label, request.AddressLine1,
            request.AddressLine2, request.City,
            request.State, request.PinCode,
            request.Landmark);

        await _db.SaveChangesAsync(ct);
        return MapAddressToDto(address);
    }

    public async Task DeleteAddressAsync(
        Guid userId, Guid addressId,
        CancellationToken ct = default)
    {
        var address = await GetAddressForUserAsync(userId, addressId, ct);

        if (address.IsDefault)
        {
            // Check if other active addresses exist
            var profile = await GetOrCreateProfileAsync(userId, ct);
            var hasOthers = await _db.CustomerAddresses
                .AnyAsync(a =>
                    a.CustomerId == profile.Id &&
                    a.IsActive && a.Id != addressId, ct);

            if (hasOthers)
                throw new AppException(ResponseMessages.AddressCannotDeleteDefault);
        }

        address.SoftDelete();
        await _db.SaveChangesAsync(ct);
    }

    public async Task<CustomerAddressDto> SetDefaultAddressAsync(
        Guid userId, Guid addressId,
        CancellationToken ct = default)
    {
        var profile = await GetOrCreateProfileAsync(userId, ct);
        var address = await GetAddressForUserAsync(userId, addressId, ct);

        // Remove existing default
        var currentDefault = await _db.CustomerAddresses
            .FirstOrDefaultAsync(a =>
                a.CustomerId == profile.Id &&
                a.IsDefault && a.IsActive, ct);

        currentDefault?.UnsetDefault();

        address.SetAsDefault();
        await _db.SaveChangesAsync(ct);

        return MapAddressToDto(address);
    }

    public async Task<IReadOnlyList<CustomerAddressDto>> GetAddressesAsync(
        Guid userId, CancellationToken ct = default)
    {
        var profile = await GetOrCreateProfileAsync(userId, ct);

        var addresses = await _db.CustomerAddresses
            .AsNoTracking()
            .Where(a => a.CustomerId == profile.Id && a.IsActive)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.CreatedAt)
            .ToListAsync(ct);

        return addresses.Select(MapAddressToDto).ToList().AsReadOnly();
    }

    public async Task<CustomerAddressDto> GetAddressByIdAsync(
        Guid userId, Guid addressId,
        CancellationToken ct = default)
    {
        var address = await GetAddressForUserAsync(userId, addressId, ct);
        return MapAddressToDto(address);
    }

    // ── Favourites ────────────────────────────────────────────────────────

    public async Task<FavouriteServiceDto> AddFavouriteAsync(
        Guid userId, Guid serviceId,
        CancellationToken ct = default)
    {
        var profile = await GetOrCreateProfileAsync(userId, ct);

        var service = await _db.Services
            .Include(s => s.Category)
            .Include(s => s.SubCategory)
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.IsActive, ct)
            ?? throw new NotFoundException(ResponseMessages.ServiceNotFound);

        // Idempotent — already favourite → return existing
        var existing = await _db.FavouriteServices
            .FirstOrDefaultAsync(f =>
                f.CustomerId == profile.Id &&
                f.ServiceId == serviceId, ct);

        if (existing is not null)
            return MapFavouriteToDto(existing, service);

        var favourite = FavouriteService.Create(profile.Id, serviceId);
        _db.FavouriteServices.Add(favourite);
        await _db.SaveChangesAsync(ct);

        return MapFavouriteToDto(favourite, service);
    }

    public async Task RemoveFavouriteAsync(
        Guid userId, Guid serviceId,
        CancellationToken ct = default)
    {
        var profile = await GetOrCreateProfileAsync(userId, ct);

        var favourite = await _db.FavouriteServices
            .FirstOrDefaultAsync(f =>
                f.CustomerId == profile.Id &&
                f.ServiceId == serviceId, ct)
            ?? throw new NotFoundException(
                "This service is not in your favourites.");

        _db.FavouriteServices.Remove(favourite);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<FavouriteServiceDto>> GetFavouritesAsync(
        Guid userId, CancellationToken ct = default)
    {
        var profile = await GetOrCreateProfileAsync(userId, ct);

        var favourites = await _db.FavouriteServices
            .AsNoTracking()
            .Include(f => f.Service)
                .ThenInclude(s => s.Category)
            .Include(f => f.Service)
                .ThenInclude(s => s.SubCategory)
            .Where(f => f.CustomerId == profile.Id
                     && f.Service.IsActive)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(ct);

        return favourites
            .Select(f => MapFavouriteToDto(f, f.Service))
            .ToList()
            .AsReadOnly();
    }

    // ── private ───────────────────────────────────────────────────────────

    private async Task<CustomerProfile> GetOrCreateProfileAsync(
        Guid userId, CancellationToken ct)
    {
        var profile = await _db.CustomerProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);

        if (profile is not null) return profile;

        // Auto-create profile from User data
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new NotFoundException("User not found.");

        var newProfile = CustomerProfile.Create(
            userId, user.FullName,
            user.PhoneNumber,
            Models.Enums.Gender.Ladies); // default, customer updates later

        _db.CustomerProfiles.Add(newProfile);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "CustomerProfile auto-created for UserId {UserId}",
            userId);

        return newProfile;
    }

    private async Task<CustomerAddress> GetAddressForUserAsync(
        Guid userId, Guid addressId, CancellationToken ct)
    {
        var profile = await GetOrCreateProfileAsync(userId, ct);

        return await _db.CustomerAddresses
            .FirstOrDefaultAsync(a =>
                a.Id == addressId &&
                a.CustomerId == profile.Id &&
                a.IsActive, ct)
            ?? throw new NotFoundException(ResponseMessages.AddressNotFound);
    }

    private static CustomerProfileDto MapToDto(CustomerProfile p) =>
        new(p.Id, p.UserId, p.FullName, p.PhoneNumber,
            p.DateOfBirth, p.Gender, p.ProfileImageUrl,
            p.IsActive, p.CreatedAt);

    private static CustomerAddressDto MapAddressToDto(CustomerAddress a) =>
        new(a.Id, a.Label, a.AddressLine1, a.AddressLine2,
            a.City, a.State, a.PinCode, a.Landmark,
            a.IsDefault, a.CreatedAt);

    private static FavouriteServiceDto MapFavouriteToDto(
        FavouriteService f, Service s) =>
        new(f.Id, s.Id, s.Name, s.Category.Name,
            s.SubCategory?.Name, s.ServiceTypeActual, s.Gender,
            s.DiscountedPrice ?? s.BasePrice,
            s.DurationMinutes, s.LoyaltyPoints,
            s.ImageUrl, f.CreatedAt);
}