namespace BeautyPlusParlour.Exceptions
{
    public class DuplicateEntryException: Exception
    {
        public DuplicateEntryException(string entity, string field, string value)
     : base($"{entity} with {field} '{value}' already exists.") { }
    }
}
