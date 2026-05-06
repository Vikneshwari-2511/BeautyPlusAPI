namespace BeautyPlusParlour.Models.DTOs.Auth;

public sealed record ResetPasswordRequest(
    string Email,
    string Otp,
    string NewPassword,
    string ConfirmNewPassword
);