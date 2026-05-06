namespace BeautyPlusParlour.Exceptions;

public sealed class ValidationException : AppException
{
    public IEnumerable<string> Errors { get; }

    public ValidationException(IEnumerable<string> errors)
        : base("One or more validation errors occurred.", 422)
    {
        Errors = errors;
    }
}