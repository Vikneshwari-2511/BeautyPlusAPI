using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.DTOs.Users;

public sealed record CreateUserRequest(
    string FullName,
    string Email,
    string Password,
    string ConfirmPassword,
    string PhoneNumber,
    UserRole Role
);