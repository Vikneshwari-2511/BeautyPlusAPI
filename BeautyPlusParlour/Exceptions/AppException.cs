namespace BeautyPlusParlour.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }

    public IEnumerable<string> Errors { get; }

    public AppException(
        string message,
        int statusCode = 400,
        IEnumerable<string>? errors = null
    ) : base(message)
    {
        StatusCode = statusCode;

        Errors = errors
            ?? Enumerable.Empty<string>();
    }
}