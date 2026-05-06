namespace BeautyPlusParlour.Models.DTOs.Auth;

public sealed record LoginRequest(
    string Email,
    string Password
);