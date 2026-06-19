namespace BeautyPlusParlour.Models.DTOs.Auth;

public sealed record FirebaseAuthResultDto(
    bool IsNewUser,
    string Message
);