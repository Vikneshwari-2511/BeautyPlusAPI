using Microsoft.AspNetCore.Http;

namespace BeautyPlusParlour.Exceptions;
public sealed class ValidationException
    : AppException
{
    public ValidationException(
        IEnumerable<string> errors
    ) : base(
        "One or more validation errors occurred.",
        StatusCodes.Status422UnprocessableEntity,
        errors
    )
    {
    }
}