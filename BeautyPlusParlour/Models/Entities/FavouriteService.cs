namespace BeautyPlusParlour.Models.Entities;

public sealed class FavouriteService
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid CustomerId { get; private set; }
    public Guid ServiceId { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation
    public CustomerProfile Customer { get; private set; } = null!;
    public Service Service { get; private set; } = null!;

    private FavouriteService() { }

    public static FavouriteService Create(Guid customerId, Guid serviceId) =>
        new() { CustomerId = customerId, ServiceId = serviceId };
}