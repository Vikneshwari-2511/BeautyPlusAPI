using BeautyPlusParlour.Exceptions;

public sealed class NotFoundException : AppException
{
    public object? Key { get; }

    public NotFoundException(string message)
        : base(message, 404) { }

    public NotFoundException(string message, object key)
        : base(message, 404)
    {
        Key = key;
    }
}