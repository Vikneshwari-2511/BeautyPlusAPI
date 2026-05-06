namespace BeautyPlusParlour.Models.Entities;

public sealed class CouponUsage
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid CouponId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid BookingId { get; private set; }
    public decimal DiscountApplied { get; private set; }
    public DateTime UsedAt { get; private set; } = DateTime.UtcNow;

    // Navigation
    public Coupon Coupon { get; private set; } = null!;
    public CustomerProfile Customer { get; private set; } = null!;
    public Booking Booking { get; private set; } = null!;

    private CouponUsage() { }

    public static CouponUsage Create(
        Guid couponId, Guid customerId,
        Guid bookingId, decimal discountApplied)
    {
        return new CouponUsage
        {
            CouponId = couponId,
            CustomerId = customerId,
            BookingId = bookingId,
            DiscountApplied = discountApplied
        };
    }
}