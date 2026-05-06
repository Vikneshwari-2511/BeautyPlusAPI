namespace BeautyPlusParlour.Interfaces;

public interface IAuditService
{
    Task LogAsync(
        Guid? userId,
        string action,
        string entityName,
        string? entityId = null,
        string? oldValues = null,
        string? newValues = null,
        CancellationToken ct = default);
}