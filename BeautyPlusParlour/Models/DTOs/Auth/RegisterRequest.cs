using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.DTOs.Auth;

public sealed record RegisterRequest(
    string FullName,
    string Email,
    string Password,
    string ConfirmPassword,
    string PhoneNumber,
    UserRole Role = UserRole.Customer
);