namespace BeautyPlusParlour.Models.DTOs.Auth;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt,
    UserDto User
);

public sealed record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string Role
);