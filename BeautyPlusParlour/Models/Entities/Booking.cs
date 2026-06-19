using BeautyPlusParlour.Models.Enums;
using Razorpay.Api;

namespace BeautyPlusParlour.Models.Entities;

public sealed class Booking
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string BookingCode { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public Guid StaffId { get; private set; }
    public Guid? AddressId { get; private set; }
    public DateOnly BookingDate { get; private set; }
    public TimeOnly BookingTime { get; private set; }
    public BookingType BookingType { get; private set; }
    public BookingStatus Status { get; private set; } = BookingStatus.Pending;
    public decimal TotalAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TravelCharge { get; private set; }
    public decimal FinalAmount { get; private set; }
    public decimal AdvanceAmount { get; private set; }
    public bool AdvancePaid { get; private set; }
    public string? CouponCode { get; private set; }
    public int LoyaltyPointsUsed { get; private set; }
    public int LoyaltyPointsEarned { get; private set; }
    public string? Notes { get; private set; }
    public string? CancellationReason { get; private set; }
    public Guid? CancelledBy { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public bool RequiresConsultation { get; private set; }
    public DateTime? ConsultationScheduledAt { get; private set; }
    public DateTime? ConsultationDoneAt { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    // Navigation
    public CustomerProfile Customer { get; private set; } = null!;
    public StaffProfile Staff { get; private set; } = null!;
    public CustomerAddress? Address { get; private set; }
    public ICollection<BookingItem> Items { get; private set; } = [];
    public ICollection<Payment> Payments { get; private set; } = [];

    private Booking() { }

    public static Booking Create(
        string bookingCode, Guid customerId, Guid staffId,
        Guid? addressId, DateOnly bookingDate, TimeOnly bookingTime,
        BookingType bookingType, decimal totalAmount,
        decimal discountAmount, decimal travelCharge,
        decimal advanceAmount, string? couponCode,
        int loyaltyPointsUsed, string? notes,
        bool requiresConsultation)
    {
        var finalAmount = totalAmount - discountAmount + travelCharge;

        return new Booking
        {
            BookingCode = bookingCode,
            CustomerId = customerId,
            StaffId = staffId,
            AddressId = addressId,
            BookingDate = bookingDate,
            BookingTime = bookingTime,
            BookingType = bookingType,
            TotalAmount = totalAmount,
            DiscountAmount = discountAmount,
            TravelCharge = travelCharge,
            FinalAmount = finalAmount,
            AdvanceAmount = advanceAmount,
            CouponCode = couponCode,
            LoyaltyPointsUsed = loyaltyPointsUsed,
            Notes = notes?.Trim(),
            RequiresConsultation = requiresConsultation
        };
    }

    public void Confirm()
    {
        Status = BookingStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }    
    public void Start()
    {
        Status = BookingStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete(int pointsEarned)
    {
        Status = BookingStatus.Completed;
        LoyaltyPointsEarned = pointsEarned;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(Guid cancelledBy, string? reason)
    {
        Status = BookingStatus.Cancelled;
        CancelledBy = cancelledBy;
        CancellationReason = reason?.Trim();
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reschedule(
        DateOnly newDate, TimeOnly newTime, Guid staffId)
    {
        BookingDate = newDate;
        BookingTime = newTime;
        StaffId = staffId;
        Status = BookingStatus.Pending;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAdvancePaid()
    {
        AdvancePaid = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ScheduleConsultation(DateTime scheduledAt)
    {
        ConsultationScheduledAt = scheduledAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CompleteConsultation()
    {
        ConsultationDoneAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanBeCancelled() =>
        Status != BookingStatus.Completed &&
        Status != BookingStatus.Cancelled;

    public bool CanBeRescheduled() =>
        Status == BookingStatus.Pending ||
        Status == BookingStatus.Confirmed;

    public int CalculateTotalPoints() =>
        Items.Sum(i => i.LoyaltyPoints);
}