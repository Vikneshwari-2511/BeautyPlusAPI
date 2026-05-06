using BeautyPlusParlour.Models.DTOs.Customer;

namespace BeautyPlusParlour.Interfaces;

public interface ICustomerService
{
    Task<CustomerProfileDto> GetMyProfileAsync(Guid userId, CancellationToken ct = default);
    Task<CustomerProfileDto> UpdateMyProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default);
    Task<CustomerProfileDto> GetByIdAsync(Guid customerId, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerListDto>> GetAllAsync(CancellationToken ct = default);
    Task DeactivateAsync(Guid customerId, Guid adminId, CancellationToken ct = default);

    // Addresses
    Task<CustomerAddressDto> AddAddressAsync(Guid userId, CreateAddressRequest request, CancellationToken ct = default);
    Task<CustomerAddressDto> UpdateAddressAsync(Guid userId, Guid addressId, UpdateAddressRequest request, CancellationToken ct = default);
    Task DeleteAddressAsync(Guid userId, Guid addressId, CancellationToken ct = default);
    Task<CustomerAddressDto> SetDefaultAddressAsync(Guid userId, Guid addressId, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerAddressDto>> GetAddressesAsync(Guid userId, CancellationToken ct = default);
    Task<CustomerAddressDto> GetAddressByIdAsync(Guid userId, Guid addressId, CancellationToken ct = default);

    // Favourites
    Task<FavouriteServiceDto> AddFavouriteAsync(Guid userId, Guid serviceId, CancellationToken ct = default);
    Task RemoveFavouriteAsync(Guid userId, Guid serviceId, CancellationToken ct = default);
    Task<IReadOnlyList<FavouriteServiceDto>> GetFavouritesAsync(Guid userId, CancellationToken ct = default);
}