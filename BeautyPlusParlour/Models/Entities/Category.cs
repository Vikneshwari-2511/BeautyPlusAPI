using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.Entities;

public sealed class Category
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ServiceType ServiceTypeDefault { get; private set; }
    public string? ImageUrl { get; private set; }
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
    public User CreatedByUser { get; private set; } = null!;
    public ICollection<SubCategory> SubCategories { get; private set; } = [];
    public ICollection<Service> Services { get; private set; } = [];

    private Category() { }

    public static Category Create(
        string name, string slug,
        string? description, ServiceType serviceTypeDefault,
        string? imageUrl, int displayOrder,
        Guid createdBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        return new Category
        {
            Name = name.Trim(),
            Slug = slug.ToLowerInvariant(),
            Description = description?.Trim(),
            ServiceTypeDefault = serviceTypeDefault,
            ImageUrl = imageUrl?.Trim(),
            DisplayOrder = displayOrder,
            CreatedBy = createdBy
        };
    }

    public void Update(
        string name, string slug,
        string? description, ServiceType serviceTypeDefault,
        string? imageUrl, int displayOrder,
        Guid updatedBy)
    {
        Name = name.Trim();
        Slug = slug.ToLowerInvariant();
        Description = description?.Trim();
        ServiceTypeDefault = serviceTypeDefault;
        ImageUrl = imageUrl?.Trim();
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