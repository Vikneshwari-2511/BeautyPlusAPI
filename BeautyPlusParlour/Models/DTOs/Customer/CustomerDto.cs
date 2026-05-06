using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.DTOs.Customer;

public sealed record CustomerProfileDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string PhoneNumber,
    DateOnly? DateOfBirth,
    Gender Gender,
    string? ProfileImageUrl,
    bool IsActive,
    DateTime CreatedAt
);

public sealed record CustomerListDto(
    Guid Id,
    string FullName,
    string PhoneNumber,
    Gender Gender,
    bool IsActive,
    DateTime CreatedAt,
    int TotalBookings
);