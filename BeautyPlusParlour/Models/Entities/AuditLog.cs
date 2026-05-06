namespace BeautyPlusParlour.Models.Entities;

public sealed class AuditLog
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid? UserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityName { get; private set; } = string.Empty;
    public string? EntityId { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public string IpAddress { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private AuditLog() { }

    public static AuditLog Create(
        Guid? userId, string action,
        string entityName, string? entityId,
        string? oldValues, string? newValues,
        string ipAddress)
    {
        return new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress
        };
    }
}