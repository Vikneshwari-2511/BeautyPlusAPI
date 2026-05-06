namespace BeautyPlusParlour.Models.DTOs.Service;

public sealed record OnSiteDetailRequest(
    decimal TravelCharge,
    int AdvancePercent,
    int MinBookingDays,
    string? SpecialNotes
);