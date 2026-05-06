using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.Entities;

public sealed class Service
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid CategoryId { get; private set; }
    public Guid? SubCategoryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ServiceType ServiceTypeActual { get; private set; }
    public Gender Gender { get; private set; }
    public decimal BasePrice { get; private set; }
    public decimal? DiscountedPrice { get; private set; }
    public int DurationMinutes { get; private set; }
    public int BufferMinutes { get; private set; }
    public int LoyaltyPoints { get; private set; }
    public bool IsLoyaltyOverridden { get; private set; }
    public string? ImageUrl { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsFeatured { get; private set; }
    public bool IsPopular { get; private set; }
    public bool RequiresConsultation { get; private set; }
    public bool IsTaxInclusive { get; private set; } = true;
    public decimal? TaxPercent { get; private set; }

    // Audit
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public Guid? UpdatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    // Navigation
    public Category Category { get; private set; } = null!;
    public SubCategory? SubCategory { get; private set; }
    public OnSiteDetail? OnSiteDetail { get; private set; }

    private Service() { }

    public static Service Create(
        Guid categoryId, Guid? subCategoryId,
        string name, string slug, string? description,
        ServiceType serviceTypeActual, Gender gender,
        decimal basePrice, decimal? discountedPrice,
        int durationMinutes, int bufferMinutes,
        int loyaltyPoints, bool isLoyaltyOverridden,
        string? imageUrl, int displayOrder,
        bool isFeatured, bool isPopular,
        bool requiresConsultation,
        bool isTaxInclusive, decimal? taxPercent,
        Guid createdBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        return new Service
        {
            CategoryId = categoryId,
            SubCategoryId = subCategoryId,
            Name = name.Trim(),
            Slug = slug.ToLowerInvariant(),
            Description = description?.Trim(),
            ServiceTypeActual = serviceTypeActual,
            Gender = gender,
            BasePrice = basePrice,
            DiscountedPrice = discountedPrice,
            DurationMinutes = durationMinutes,
            BufferMinutes = bufferMinutes,
            LoyaltyPoints = loyaltyPoints,
            IsLoyaltyOverridden = isLoyaltyOverridden,
            ImageUrl = imageUrl?.Trim(),
            DisplayOrder = displayOrder,
            IsFeatured = isFeatured,
            IsPopular = isPopular,
            RequiresConsultation = requiresConsultation,
            IsTaxInclusive = isTaxInclusive,
            TaxPercent = taxPercent,
            CreatedBy = createdBy
        };
    }

    public void Update(
        Guid categoryId, Guid? subCategoryId,
        string name, string slug, string? description,
        ServiceType serviceTypeActual, Gender gender,
        decimal basePrice, decimal? discountedPrice,
        int durationMinutes, int bufferMinutes,
        int loyaltyPoints, bool isLoyaltyOverridden,
        string? imageUrl, int displayOrder,
        bool isFeatured, bool isPopular,
        bool requiresConsultation,
        bool isTaxInclusive, decimal? taxPercent,
        Guid updatedBy)
    {
        CategoryId = categoryId;
        SubCategoryId = subCategoryId;
        Name = name.Trim();
        Slug = slug.ToLowerInvariant();
        Description = description?.Trim();
        ServiceTypeActual = serviceTypeActual;
        Gender = gender;
        BasePrice = basePrice;
        DiscountedPrice = discountedPrice;
        DurationMinutes = durationMinutes;
        BufferMinutes = bufferMinutes;
        LoyaltyPoints = loyaltyPoints;
        IsLoyaltyOverridden = isLoyaltyOverridden;
        ImageUrl = imageUrl?.Trim();
        DisplayOrder = displayOrder;
        IsFeatured = isFeatured;
        IsPopular = isPopular;
        RequiresConsultation = requiresConsultation;
        IsTaxInclusive = isTaxInclusive;
        TaxPercent = taxPercent;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ToggleActive(Guid updatedBy)
    {
        IsActive = !IsActive;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete(Guid deletedBy)
    {
        IsActive = false;
        DeletedBy = deletedBy;
        DeletedAt = DateTime.UtcNow;
    }

    // Auto-calculate loyalty points based on price and type
    public static int CalculateLoyaltyPoints(
        decimal basePrice, ServiceType serviceType)
    {
        var basePoints = (int)Math.Floor(basePrice / ServiceConstants.LoyaltyPointsDivisor);

        return serviceType == ServiceType.OnSite
            ? basePoints * ServiceConstants.OnSiteLoyaltyMultiplier
            : basePoints;
    }
}