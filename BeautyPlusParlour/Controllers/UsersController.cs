using BeautyPlusParlour.Models.DTOs.Common;
using BeautyPlusParlour.Models.DTOs.Users;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BeautyPlusParlour.Interfaces;
namespace BeautyPlusParlour.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = "Admin")]
    public sealed class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IValidator<CreateUserRequest> _validator;

        public UsersController(
            IUserService userService,
            IValidator<CreateUserRequest> validator)
        {
            _userService = userService;
            _validator = validator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(
            [FromBody] CreateUserRequest request,
            CancellationToken ct)
        {
            var validation =
                await _validator.ValidateAsync(
                    request, ct);

            if (!validation.IsValid)
            {
                return UnprocessableEntity(
                    ApiResponse<object>.Fail(
                        "Validation failed.",
                        validation.Errors
                            .Select(x => x.ErrorMessage)));
            }

            var adminId = Guid.Parse(
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!);

            var response =
                await _userService.AdminCreateUserAsync(
                    request,
                    adminId,
                    ct);

            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<UserListDto>.Ok(
                    response,
                    "User created successfully."));
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(
            CancellationToken ct)
        {
            var users =
                await _userService.GetAllAsync(ct);

            return Ok(
                ApiResponse<IReadOnlyList<UserListDto>>
                .Ok(users));
        }
    }
}
