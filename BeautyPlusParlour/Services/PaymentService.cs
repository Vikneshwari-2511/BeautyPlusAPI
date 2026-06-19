using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BeautyPlusParlour.Data;
using BeautyPlusParlour.Exceptions;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Payment;
using BeautyPlusParlour.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Razorpay.Api;
using BeautyPlusParlour.Models.Entities;

namespace BeautyPlusParlour.Services;

public sealed class PaymentService : IPaymentService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly IConfiguration _config;
    private readonly ILogger<PaymentService> _logger;

    private readonly string _keyId;
    private readonly string _keySecret;
    private readonly string _webhookSecret;

    public PaymentService(
        AppDbContext db,
        IEmailService email,
        IConfiguration config,
        ILogger<PaymentService> logger)
    {
        _db = db;
        _email = email;
        _config = config;
        _logger = logger;

        _keyId = config["Razorpay:KeyId"]!;
        _keySecret = config["Razorpay:KeySecret"]!;
        _webhookSecret = config["Razorpay:WebhookSecret"]!;
    }

    // ── Create Razorpay Order ─────────────────────────────
    public async Task<CreatePaymentOrderResponse> CreateOrderAsync(
        Guid bookingId, Guid userId,
        CancellationToken ct = default)
    {
        // Load booking
        var booking = await _db.Bookings
            .Include(b => b.Customer)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(
                b => b.Id == bookingId, ct)
            ?? throw new NotFoundException(
                "Booking not found.");

        // Validate ownership
        if (booking.Customer.UserId != userId)
            throw new UnauthorizedException(
                "Not your booking.");

        // Check booking status
        if (booking.Status != BookingStatus.Pending)
            throw new AppException(
                "Booking is not in a payable state.");

        // Check if payment already exists
        var existingPayment = await _db.Payments
            .FirstOrDefaultAsync(
                p => p.BookingId == bookingId &&
                     p.Status == PaymentStatus.Captured, ct);

        if (existingPayment != null)
            throw new AppException(
                "Payment already completed for this booking.");

        // Amount in paise (multiply by 100)
        var amountInPaise =
            (long)(booking.FinalAmount * 100);

        // Create Razorpay order
        var client = new RazorpayClient(_keyId, _keySecret);

        var options = new Dictionary<string, object>
        {
            ["amount"] = amountInPaise,
            ["currency"] = "INR",
            ["receipt"] = booking.BookingCode,
            ["notes"] = new Dictionary<string, string>
            {
                ["booking_id"] = booking.Id.ToString(),
                ["booking_code"] = booking.BookingCode
            }
        };

        var order = client.Order.Create(options);
        var orderId = order["id"].ToString();

        // Save payment record
        var payment = Models.Entities.Payment.Create(
            bookingId: booking.Id,
            razorpayOrderId: orderId,
            amount: booking.FinalAmount,
            currency: "INR");

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
    "Razorpay order created: {OrderId} for booking: {BookingCode}",
    (object)orderId, (object)booking.BookingCode);

        return new CreatePaymentOrderResponse(
            OrderId: orderId,
            RazorpayKeyId: _keyId,
            Amount: booking.FinalAmount,
            Currency: "INR",
            BookingCode: booking.BookingCode,
            CustomerName: booking.Customer.User.FullName,
            CustomerEmail: booking.Customer.User.Email,
            CustomerPhone: booking.Customer.User.PhoneNumber,
            Description: $"Beauty Plus Parlour — {booking.BookingCode}"
        );
    }

    // ── Verify Payment ────────────────────────────────────
    public async Task<VerifyPaymentResponse> VerifyPaymentAsync(
        VerifyPaymentRequest request,
        Guid userId,
        CancellationToken ct = default)
    {
        // 1. Verify Razorpay signature
        var isValid = VerifySignature(
            request.RazorpayOrderId,
            request.RazorpayPaymentId,
            request.RazorpaySignature);

        if (!isValid)
        {
            _logger.LogWarning(
                "Invalid Razorpay signature for order: {OrderId}",
                request.RazorpayOrderId);

            await MarkPaymentFailed(
                request.BookingId,
                request.RazorpayOrderId,
                "Invalid signature",
                ct);

            throw new AppException(
                "Payment verification failed. " +
                "Invalid signature.");
        }

        // 2. Load booking + payment
        var booking = await _db.Bookings
            .Include(b => b.Customer)
                .ThenInclude(c => c.User)
            .Include(b => b.Items)
                .ThenInclude(i => i.Service)
            .FirstOrDefaultAsync(
                b => b.Id == request.BookingId, ct)
            ?? throw new NotFoundException("Booking not found.");

        var payment = await _db.Payments
            .FirstOrDefaultAsync(
                p => p.BookingId == request.BookingId &&
                     p.RazorpayOrderId == request.RazorpayOrderId,
                ct)
            ?? throw new NotFoundException("Payment record not found.");

        // 3. Update payment record
        payment.MarkCaptured(
            request.RazorpayPaymentId,
            DateTime.UtcNow);

        // 4. Update booking status → Confirmed
        booking.Confirm();

        // 5. Earn loyalty points
        if (booking.LoyaltyPointsEarned > 0)
        {
            var loyalty = await _db.CustomerLoyaltyPoints
                .FirstOrDefaultAsync(
                    l => l.CustomerId == booking.CustomerId, ct);

            if (loyalty != null)
            {
                loyalty.AddPoints(booking.LoyaltyPointsEarned);
            }
        }

        await _db.SaveChangesAsync(ct);

        // 6. Send confirmation email
        try
        {
            await _email.SendBookingConfirmationAsync(
                booking.Customer.User.Email,
                booking.Customer.User.FullName,
                booking.BookingCode,
                booking.BookingDate.ToString("dd MMM yyyy"),
                booking.BookingTime.ToString("hh:mm tt"),
                booking.FinalAmount,
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, "Failed to send confirmation email " +
                "for booking {Code}", booking.BookingCode);
        }

        _logger.LogInformation(
            "Payment verified for booking {Code}. " +
            "PaymentId: {PaymentId}",
            booking.BookingCode,
            request.RazorpayPaymentId);

        return new VerifyPaymentResponse(
            Success: true,
            Message: "Payment successful! Booking confirmed.",
            BookingCode: booking.BookingCode,
            BookingStatus: "Confirmed"
        );
    }

    // ── Webhook Handler ───────────────────────────────────
    public async Task HandleWebhookAsync(
        string payload,
        string signature,
        CancellationToken ct = default)
    {
        // Verify webhook signature
        var isValid = VerifyWebhookSignature(
            payload, signature);

        if (!isValid)
        {
            _logger.LogWarning("Invalid webhook signature.");
            throw new UnauthorizedException(
                "Invalid webhook signature.");
        }

        var json = JsonDocument.Parse(payload);
        var eventType = json.RootElement
            .GetProperty("event").GetString();

        _logger.LogInformation(
            "Razorpay webhook received: {Event}", eventType);

        switch (eventType)
        {
            case "payment.captured":
                await HandlePaymentCaptured(json, ct);
                break;

            case "payment.failed":
                await HandlePaymentFailed(json, ct);
                break;

            default:
                _logger.LogInformation(
                    "Unhandled webhook event: {Event}", eventType);
                break;
        }
    }

    // ── Get Payment ───────────────────────────────────────
    public async Task<PaymentDetailsDto?> GetByBookingIdAsync(
        Guid bookingId, CancellationToken ct = default)
    {
        var payment = await _db.Payments
            .Include(p => p.Booking)
            .FirstOrDefaultAsync(
                p => p.BookingId == bookingId, ct);

        if (payment is null) return null;

        return new PaymentDetailsDto(
            Id: payment.Id,
            BookingId: payment.BookingId,
            BookingCode: payment.Booking.BookingCode,
            RazorpayOrderId: payment.RazorpayOrderId,
            RazorpayPaymentId: payment.RazorpayPaymentId,
            Amount: payment.Amount,
            Currency: payment.Currency,
            Status: payment.Status.ToString(),
            Method: payment.Method ?? "Card/UPI/NetBanking",
            FailureReason: payment.FailureReason,
            CreatedAt: payment.CreatedAt,
            PaidAt: payment.PaidAt
        );
    }

    // ── Private Helpers ───────────────────────────────────
    private bool VerifySignature(
        string orderId,
        string paymentId,
        string signature)
    {
        var payload = $"{orderId}|{paymentId}";
        var key = Encoding.UTF8.GetBytes(_keySecret);
        var data = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(data);
        var computed = Convert.ToHexString(hash).ToLower();

        return computed == signature;
    }

    private bool VerifyWebhookSignature(
        string payload, string signature)
    {
        var key = Encoding.UTF8.GetBytes(_webhookSecret);
        var data = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(data);
        var computed = Convert.ToHexString(hash).ToLower();

        return computed == signature;
    }

    private async Task HandlePaymentCaptured(
        JsonDocument json, CancellationToken ct)
    {
        try
        {
            var paymentEntity = json.RootElement
                .GetProperty("payload")
                .GetProperty("payment")
                .GetProperty("entity");

            var orderId = paymentEntity
                .GetProperty("order_id").GetString()!;
            var paymentId = paymentEntity
                .GetProperty("id").GetString()!;

            var payment = await _db.Payments
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(
                    p => p.RazorpayOrderId == orderId, ct);

            if (payment is null) return;

            if (payment.Status != PaymentStatus.Captured)
            {
                payment.MarkCaptured(
                    paymentId, DateTime.UtcNow);
                payment.Booking.Confirm();
                await _db.SaveChangesAsync(ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, "Error handling payment.captured webhook");
        }
    }

    private async Task HandlePaymentFailed(
        JsonDocument json, CancellationToken ct)
    {
        try
        {
            var paymentEntity = json.RootElement
                .GetProperty("payload")
                .GetProperty("payment")
                .GetProperty("entity");

            var orderId = paymentEntity
                .GetProperty("order_id").GetString()!;
            var errorDesc = paymentEntity
                .GetProperty("error_description")
                .GetString() ?? "Payment failed";

            var payment = await _db.Payments
                .FirstOrDefaultAsync(
                    p => p.RazorpayOrderId == orderId, ct);

            if (payment is null) return;

            payment.MarkFailed(errorDesc);
            await _db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, "Error handling payment.failed webhook");
        }
    }

    private async Task MarkPaymentFailed(
        Guid bookingId, string orderId,
        string reason, CancellationToken ct)
    {
        var payment = await _db.Payments
            .FirstOrDefaultAsync(
                p => p.BookingId == bookingId &&
                     p.RazorpayOrderId == orderId, ct);

        if (payment != null)
        {
            payment.MarkFailed(reason);
            await _db.SaveChangesAsync(ct);
        }
    }
}