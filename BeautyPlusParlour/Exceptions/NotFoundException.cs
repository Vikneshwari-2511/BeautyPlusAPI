using Microsoft.AspNetCore.Http;

namespace BeautyPlusParlour.Exceptions;

public sealed class NotFoundException
    : AppException
{
    public object? Key { get; }

    public NotFoundException(
        string message
    ) : base(
        message,
        StatusCodes.Status404NotFound
    )
    {
    }

    public NotFoundException(
        string message,
        object key
    ) : base(
        message,
        StatusCodes.Status404NotFound
    )
    {
        Key = key;
    }
}