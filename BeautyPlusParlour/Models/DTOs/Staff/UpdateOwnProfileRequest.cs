namespace BeautyPlusParlour.Models.DTOs.Staff;

public sealed record UpdateOwnProfileRequest(
    string? ProfileImageUrl,
    string? Bio,
    string PhoneNumber,
    string? AlternatePhone
);