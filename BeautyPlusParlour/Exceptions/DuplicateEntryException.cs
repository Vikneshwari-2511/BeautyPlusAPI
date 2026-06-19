namespace BeautyPlusParlour.Exceptions;

public sealed class DuplicateEntryException
    : AppException
{
    public DuplicateEntryException(
        string entity,
        string field,
        string value
    ) : base(
        $"{entity} with {field} '{value}' already exists.",
        409
    )
    {
    }
}