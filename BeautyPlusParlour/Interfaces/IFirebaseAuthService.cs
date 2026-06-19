using BeautyPlusParlour.Models.DTOs.Auth;

namespace BeautyPlusParlour.Interfaces;

public interface IFirebaseAuthService
{
    Task<AuthResponse> VerifyAndLoginAsync(
        FirebasePhoneLoginRequest request,
        string ipAddress,
        string userAgent,
        CancellationToken ct = default);
}