namespace BeautyPlusParlour.Models.Entities;

public sealed class BookingItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid BookingId { get; private set; }
    public Guid ServiceId { get; private set; }
    public string ServiceName { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int DurationMinutes { get; private set; }
    public int BufferMinutes { get; private set; }
    public int LoyaltyPoints { get; private set; }

    // Navigation
    public Booking Booking { get; private set; } = null!;
    public Service Service { get; private set; } = null!;

    private BookingItem() { }

    public static BookingItem Create(
        Guid bookingId, Guid serviceId,
        string serviceName, decimal price,
        int durationMinutes, int bufferMinutes,
        int loyaltyPoints)
    {
        return new BookingItem
        {
            BookingId = bookingId,
            ServiceId = serviceId,
            ServiceName = serviceName,
            Price = price,
            DurationMinutes = durationMinutes,
            BufferMinutes = bufferMinutes,
            LoyaltyPoints = loyaltyPoints
        };
    }
}