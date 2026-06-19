namespace BeautyPlusParlour.Models.DTOs.Auth;

public sealed record FirebasePhoneLoginRequest(
    string FirebaseToken,
    string PhoneNumber
);