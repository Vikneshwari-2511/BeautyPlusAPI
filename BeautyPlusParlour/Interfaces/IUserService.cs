using BeautyPlusParlour.Models.DTOs.Auth;
using BeautyPlusParlour.Models.DTOs.Users;

public interface IUserService
{
    Task<UserListDto> AdminCreateUserAsync(
        CreateUserRequest request,
        Guid adminId,
        CancellationToken ct = default);

    Task<IReadOnlyList<UserListDto>> GetAllAsync(
        CancellationToken ct = default);
}