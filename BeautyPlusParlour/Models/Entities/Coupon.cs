using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.Entities;

public sealed class Coupon
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public CouponType CouponType { get; private set; }
    public decimal Value { get; private set; }
    public decimal MinOrderAmount { get; private set; }
    public decimal? MaxDiscount { get; private set; }
    public int? UsageLimit { get; private set; }
    public int PerUserLimit { get; private set; }
    public int UsedCount { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime ValidTo { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    // Navigation
    public ICollection<CouponUsage> Usages { get; private set; } = [];

    private Coupon() { }

    public static Coupon Create(
        string code, string description,
        CouponType couponType, decimal value,
        decimal minOrderAmount, decimal? maxDiscount,
        int? usageLimit, int perUserLimit,
        DateTime validFrom, DateTime validTo,
        Guid createdBy)
    {
        return new Coupon
        {
            Code = code.ToUpperInvariant().Trim(),
            Description = description.Trim(),
            CouponType = couponType,
            Value = value,
            MinOrderAmount = minOrderAmount,
            MaxDiscount = maxDiscount,
            UsageLimit = usageLimit,
            PerUserLimit = perUserLimit,
            ValidFrom = validFrom,
            ValidTo = validTo,
            CreatedBy = createdBy
        };
    }

    public void Update(
        string description, decimal value,
        decimal minOrderAmount, decimal? maxDiscount,
        int? usageLimit, int perUserLimit,
        DateTime validFrom, DateTime validTo)
    {
        Description = description.Trim();
        Value = value;
        MinOrderAmount = minOrderAmount;
        MaxDiscount = maxDiscount;
        UsageLimit = usageLimit;
        PerUserLimit = perUserLimit;
        ValidFrom = validFrom;
        ValidTo = validTo;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate() => IsActive = false;

    public void IncrementUsage()
    {
        UsedCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsValid() => IsActive && DateTime.UtcNow >= ValidFrom && DateTime.UtcNow <= ValidTo;
    public bool IsExpired() => DateTime.UtcNow > ValidTo;
    public bool IsLimitReached() => UsageLimit.HasValue && UsedCount >= UsageLimit;

    public decimal CalculateDiscount(decimal orderAmount)
    {
        if (CouponType == CouponType.Fixed)
            return Math.Min(Value, orderAmount);

        var discount = Math.Round(orderAmount * (Value / 100), 2);

        return MaxDiscount.HasValue
            ? Math.Min(discount, MaxDiscount.Value)
            : discount;
    }
}