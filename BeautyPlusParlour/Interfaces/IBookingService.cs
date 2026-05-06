using BeautyPlusParlour.Models.DTOs.Booking;
using BeautyPlusParlour.Models.DTOs.Common;
using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Interfaces;

public interface IBookingService
{
    Task<BookingDto> CreateAsync(Guid userId, CreateBookingRequest request, CancellationToken ct = default);
    Task<BookingDto> GetByIdAsync(Guid bookingId, Guid userId, bool isAdmin, CancellationToken ct = default);
    Task<PagedResponse<BookingListDto>> GetAllAsync(BookingStatus? status, DateOnly? date, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<BookingListDto>> GetMyBookingsAsync(Guid userId, CancellationToken ct = default);
    Task<BookingDto> ConfirmAsync(Guid bookingId, Guid adminId, CancellationToken ct = default);
    Task<BookingDto> StartAsync(Guid bookingId, Guid staffUserId, CancellationToken ct = default);
    Task<BookingDto> CompleteAsync(Guid bookingId, Guid staffUserId, CancellationToken ct = default);
    Task<BookingDto> CancelAsync(Guid bookingId, Guid userId, bool isAdmin, CancelBookingRequest request, CancellationToken ct = default);
    Task<BookingDto> RescheduleAsync(Guid bookingId, Guid userId, RescheduleBookingRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<AvailableSlotDto>> GetAvailableSlotsAsync(AvailableSlotsRequest request, CancellationToken ct = default);
    Task<PaymentDto> RecordPaymentAsync(Guid bookingId, Guid adminId, RecordPaymentRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<PaymentDto>> GetPaymentsAsync(Guid bookingId, CancellationToken ct = default);
    Task<BookingDto> ScheduleConsultationAsync(Guid bookingId, Guid adminId, ScheduleConsultationRequest request, CancellationToken ct = default);
    Task<BookingDto> CompleteConsultationAsync(Guid bookingId, Guid adminId, CancellationToken ct = default);
}