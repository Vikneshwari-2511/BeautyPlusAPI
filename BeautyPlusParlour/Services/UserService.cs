using BeautyPlusParlour.Configurations;
using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Data;
using BeautyPlusParlour.Exceptions;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Users;
using BeautyPlusParlour.Models.Entities;
using BeautyPlusParlour.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BeautyPlusParlour.Services
{
    public class UserService:IUserService
    {
        private readonly AppDbContext _db;
        private readonly IAuditService _audit;        
        private readonly ILogger<UserService> _logger;

        public UserService(
            AppDbContext db,

            IAuditService audit,
            ILogger<UserService> logger)
        {
            _db = db;
            _audit = audit;                       
            _logger = logger;
        }

        public async Task<UserListDto> AdminCreateUserAsync(
    CreateUserRequest request,
    Guid adminId,
    CancellationToken ct = default)
        {
            var exists = await _db.Users
                .AnyAsync(u =>
                    u.Email == request.Email.ToLowerInvariant(),
                    ct);

            if (exists)
                throw new AppException(
                    ResponseMessages.EmailAlreadyExists);

            var hash = BCrypt.Net.BCrypt.HashPassword(
                request.Password,
                workFactor: 12);

            var user = User.Create(
                request.FullName,
                request.Email,
                hash,
                request.PhoneNumber,
                request.Role);

            _db.Users.Add(user);

            await _db.SaveChangesAsync(ct);

            await _audit.LogAsync(
                adminId,
                AuditAction.Created,
                "User",
                user.Id.ToString(),
                null,
                user.Email,
                ct);

            return new UserListDto(
                user.Id,
                user.FullName,
                user.Email,
                user.PhoneNumber,
                user.Role,
                user.IsEmailVerified,
                user.IsActive,
                user.CreatedAt);
        }

        public async Task<IReadOnlyList<UserListDto>> GetAllAsync(
        CancellationToken ct = default)
        {
            return await _db.Users
                .AsNoTracking()
                .OrderBy(x => x.FullName)
                .Select(x => new UserListDto(
                    x.Id,
                    x.FullName,
                    x.Email,
                    x.PhoneNumber,
                    x.Role,
                    x.IsEmailVerified,
                    x.IsActive,
                    x.CreatedAt))
                .ToListAsync(ct);
        }
    }
}
