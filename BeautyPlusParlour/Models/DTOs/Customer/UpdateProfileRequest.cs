using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.DTOs.Customer;

public sealed record UpdateProfileRequest(
    string FullName,
    string PhoneNumber,
    DateOnly? DateOfBirth,
    Gender Gender,
    string? ProfileImageUrl
);