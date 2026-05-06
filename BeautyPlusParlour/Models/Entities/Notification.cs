namespace BeautyPlusParlour.Models.Entities;

public sealed class Notification
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public Guid? ReferenceId { get; private set; }
    public string? ReferenceType { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; private set; } = null!;

    private Notification() { }

    public static Notification Create(
        Guid userId, string title, string body,
        string type, Guid? referenceId = null,
        string? referenceType = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        return new Notification
        {
            UserId = userId,
            Title = title.Trim(),
            Body = body.Trim(),
            Type = type,
            ReferenceId = referenceId,
            ReferenceType = referenceType
        };
    }

    public void MarkRead()
    {
        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }
}