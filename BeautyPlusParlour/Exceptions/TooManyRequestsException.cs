namespace BeautyPlusParlour.Exceptions;

public sealed class TooManyRequestsException : AppException
{
    public TooManyRequestsException(
        string message =
            "Too many requests. Please slow down."
    ) : base(message, 429)
    {
    }
}