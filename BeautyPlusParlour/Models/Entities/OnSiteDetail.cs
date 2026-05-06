namespace BeautyPlusParlour.Models.Entities;

public sealed class OnSiteDetail
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ServiceId { get; private set; }
    public decimal TravelCharge { get; private set; }
    public int AdvancePercent { get; private set; }
    public int MinBookingDays { get; private set; }
    public string? SpecialNotes { get; private set; }

    // Navigation
    public Service Service { get; private set; } = null!;

    private OnSiteDetail() { }

    public static OnSiteDetail Create(
        Guid serviceId, decimal travelCharge,
        int advancePercent, int minBookingDays,
        string? specialNotes)
    {
        return new OnSiteDetail
        {
            ServiceId = serviceId,
            TravelCharge = travelCharge,
            AdvancePercent = advancePercent,
            MinBookingDays = minBookingDays,
            SpecialNotes = specialNotes?.Trim()
        };
    }

    public void Update(
        decimal travelCharge, int advancePercent,
        int minBookingDays, string? specialNotes)
    {
        TravelCharge = travelCharge;
        AdvancePercent = advancePercent;
        MinBookingDays = minBookingDays;
        SpecialNotes = specialNotes?.Trim();
    }
}