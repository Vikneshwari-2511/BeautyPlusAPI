using BeautyPlusParlour.Extensions;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Common;
using BeautyPlusParlour.Models.DTOs.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeautyPlusParlour.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentController : ControllerBase
{
    private readonly IPaymentService _payment;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IPaymentService payment,
        ILogger<PaymentController> logger)
    {
        _payment = payment;
        _logger = logger;
    }

    // POST /api/payments/create-order
    [HttpPost("create-order")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreatePaymentOrderRequest request,
        CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _payment.CreateOrderAsync(
            request.BookingId, userId, ct);
        return Ok(ApiResponse<CreatePaymentOrderResponse>
            .Ok(result, "Payment order created."));
    }

    // POST /api/payments/verify
    [HttpPost("verify")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Verify(
        [FromBody] VerifyPaymentRequest request,
        CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _payment.VerifyPaymentAsync(
            request, userId, ct);
        return Ok(ApiResponse<VerifyPaymentResponse>
            .Ok(result, result.Message));
    }

    // POST /api/payments/webhook
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(CancellationToken ct)
    {
        Request.EnableBuffering();
        var body = await new StreamReader(
            Request.Body).ReadToEndAsync(ct);
        Request.Body.Position = 0;

        var signature = Request.Headers[
            "X-Razorpay-Signature"].ToString();

        if (string.IsNullOrEmpty(signature))
            return BadRequest("Missing signature.");

        await _payment.HandleWebhookAsync(body, signature, ct);
        return Ok(new { status = "ok" });
    }

    // GET /api/payments/booking/{bookingId}
    [HttpGet("booking/{bookingId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetByBooking(
        Guid bookingId,
        CancellationToken ct)
    {
        var result = await _payment
            .GetByBookingIdAsync(bookingId, ct);

        if (result is null)
            return NotFound(ApiResponse<object>
                .Fail("Payment not found."));

        return Ok(ApiResponse<PaymentDetailsDto>
            .Ok(result, "Payment details."));
    }
}