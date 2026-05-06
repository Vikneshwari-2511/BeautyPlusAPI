namespace BeautyPlusParlour.Models.Entities;

public sealed class SubCategory
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Audit
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public Guid? UpdatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    // Navigation
    public Category Category { get; private set; } = null!;
    public ICollection<Service> Services { get; private set; } = [];

    private SubCategory() { }

    public static SubCategory Create(
        Guid categoryId, string name,
        string slug, int displayOrder,
        Guid createdBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        return new SubCategory
        {
            CategoryId = categoryId,
            Name = name.Trim(),
            Slug = slug.ToLowerInvariant(),
            DisplayOrder = displayOrder,
            CreatedBy = createdBy
        };
    }

    public void Update(
        string name, string slug,
        int displayOrder, Guid updatedBy)
    {
        Name = name.Trim();
        Slug = slug.ToLowerInvariant();
        DisplayOrder = displayOrder;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete(Guid deletedBy)
    {
        IsActive = false;
        DeletedBy = deletedBy;
        DeletedAt = DateTime.UtcNow;
    }

    public void Reactivate(Guid updatedBy)
    {
        IsActive = true;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}