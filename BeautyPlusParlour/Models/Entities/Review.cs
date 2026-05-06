namespace BeautyPlusParlour.Models.Entities;

public sealed class Review
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid BookingId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid StaffId { get; private set; }
    public Guid ServiceId { get; private set; }
    public int ServiceRating { get; private set; }
    public int StaffRating { get; private set; }
    public string? Comment { get; private set; }
    public bool IsVisible { get; private set; } = true;
    public Guid? HiddenBy { get; private set; }
    public DateTime? HiddenAt { get; private set; }
    public string? HideReason { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    // Navigation
    public Booking Booking { get; private set; } = null!;
    public CustomerProfile Customer { get; private set; } = null!;
    public StaffProfile Staff { get; private set; } = null!;
    public Service Service { get; private set; } = null!;

    private Review() { }

    public static Review Create(
        Guid bookingId, Guid customerId,
        Guid staffId, Guid serviceId,
        int serviceRating, int staffRating,
        string? comment)
    {
        return new Review
        {
            BookingId = bookingId,
            CustomerId = customerId,
            StaffId = staffId,
            ServiceId = serviceId,
            ServiceRating = serviceRating,
            StaffRating = staffRating,
            Comment = comment?.Trim()
        };
    }

    public void Update(
        int serviceRating, int staffRating,
        string? comment)
    {
        ServiceRating = serviceRating;
        StaffRating = staffRating;
        Comment = comment?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Hide(Guid hiddenBy, string? reason)
    {
        IsVisible = false;
        HiddenBy = hiddenBy;
        HiddenAt = DateTime.UtcNow;
        HideReason = reason?.Trim();
    }

    public void Unhide()
    {
        IsVisible = true;
        HiddenBy = null;
        HiddenAt = null;
        HideReason = null;
    }

    public bool CanBeEdited() =>
        DateTime.UtcNow <= CreatedAt.AddHours(24);
}