using BeautyPlusParlour.Models.DTOs.Payment;

namespace BeautyPlusParlour.Interfaces;

public interface IPaymentService
{
    Task<CreatePaymentOrderResponse> CreateOrderAsync(
        Guid bookingId,
        Guid userId,
        CancellationToken ct = default);

    Task<VerifyPaymentResponse> VerifyPaymentAsync(
        VerifyPaymentRequest request,
        Guid userId,
        CancellationToken ct = default);

    Task HandleWebhookAsync(
        string payload,
        string signature,
        CancellationToken ct = default);

    Task<PaymentDetailsDto?> GetByBookingIdAsync(
        Guid bookingId,
        CancellationToken ct = default);
}