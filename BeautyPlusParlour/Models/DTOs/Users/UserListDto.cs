using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.DTOs.Users;

public sealed record UserListDto(
    Guid Id,
    string FullName,
    string Email,
    string PhoneNumber,
    UserRole Role,
    bool IsEmailVerified,
    bool IsActive,
    DateTime CreatedAt
);