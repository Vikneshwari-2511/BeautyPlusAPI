using Microsoft.AspNetCore.Http;

namespace BeautyPlusParlour.Exceptions;

public sealed class ForbiddenException
    : AppException
{
    public ForbiddenException(
        string message = "Access denied."
    ) : base(
        message,
        StatusCodes.Status403Forbidden
    )
    {
    }
}