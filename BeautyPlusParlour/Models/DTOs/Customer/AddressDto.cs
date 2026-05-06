namespace BeautyPlusParlour.Models.DTOs.Customer;

public sealed record CreateAddressRequest(
    string Label,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PinCode,
    string? Landmark
);

public sealed record UpdateAddressRequest(
    string Label,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PinCode,
    string? Landmark
);

public sealed record CustomerAddressDto(
    Guid Id,
    string Label,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PinCode,
    string? Landmark,
    bool IsDefault,
    DateTime CreatedAt
);