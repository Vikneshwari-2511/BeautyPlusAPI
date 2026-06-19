using Microsoft.AspNetCore.Http;

namespace BeautyPlusParlour.Exceptions;

public sealed class UnauthorizedException
    : AppException
{
    public UnauthorizedException(
        string message = "Unauthorized."
    ) : base(
        message,
        StatusCodes.Status401Unauthorized
    )
    {
    }
}