using System.Security.Claims;
using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Booking;
using BeautyPlusParlour.Models.DTOs.Common;
using BeautyPlusParlour.Models.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeautyPlusParlour.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class BookingController : ControllerBase
{
    private readonly IBookingService _service;
    private readonly IValidator<CreateBookingRequest> _createV;
    private readonly IValidator<RescheduleBookingRequest> _rescheduleV;
    private readonly IValidator<RecordPaymentRequest> _paymentV;

    public BookingController(
        IBookingService service,
        IValidator<CreateBookingRequest> createV,
        IValidator<RescheduleBookingRequest> rescheduleV,
        IValidator<RecordPaymentRequest> paymentV)
    {
        _service = service;
        _createV = createV;
        _rescheduleV = rescheduleV;
        _paymentV = paymentV;
    }

    // ── POST /api/bookings ────────────────────────────────────────────────
    [HttpPost]
    [Authorize(Policy = AppRoles.CustomerOnly)]
    [ProducesResponseType(typeof(ApiResponse<BookingDto>), 201)]
    public async Task<IActionResult> Create(
        [FromBody] CreateBookingRequest request,
        CancellationToken ct)
    {
        var v = await _createV.ValidateAsync(request, ct);
        if (!v.IsValid)
            return UnprocessableEntity(
                ApiResponse<object>.Fail("Validation failed.",
                    v.Errors.Select(e => e.ErrorMessage)));

        var result = await _service.CreateAsync(GetUserId(), request, ct);

        return StatusCode(201,
            ApiResponse<BookingDto>.Ok(result, ResponseMessages.BookingCreated));
    }

    // ── GET /api/bookings ─────────────────────────────────────────────────
    [HttpGet]
    [Authorize(Policy = AppRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<BookingListDto>>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] BookingStatus? status,
        [FromQuery] DateOnly? date,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(
            status, date, page, pageSize, ct);

        return Ok(ApiResponse<PagedResponse<BookingListDto>>.Ok(
            result, ResponseMessages.BookingListFetched));
    }

    // ── GET /api/bookings/my ──────────────────────────────────────────────
    [HttpGet("my")]
    [Authorize(Policy = AppRoles.CustomerOnly)]
    public async Task<IActionResult> GetMyBookings(CancellationToken ct)
    {
        var result = await _service.GetMyBookingsAsync(GetUserId(), ct);
        return Ok(ApiResponse<IReadOnlyList<BookingListDto>>.Ok(
            result, ResponseMessages.BookingListFetched));
    }

    // ── GET /api/bookings/available-slots ─────────────────────────────────
    [HttpGet("available-slots")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailableSlots(
        [FromQuery] Guid serviceId,
        [FromQuery] Guid staffId,
        [FromQuery] DateOnly date,
        CancellationToken ct)
    {
        var request = new AvailableSlotsRequest(serviceId, staffId, date);
        var result = await _service.GetAvailableSlotsAsync(request, ct);

        return Ok(ApiResponse<IReadOnlyList<AvailableSlotDto>>.Ok(
            result, ResponseMessages.SlotsFetched));
    }

    // ── GET /api/bookings/{id} ────────────────────────────────────────────
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(
        Guid id, CancellationToken ct)
    {
        var isAdmin = User.IsInRole(AppRoles.Admin);
        var result = await _service.GetByIdAsync(id, GetUserId(), isAdmin, ct);

        return Ok(ApiResponse<BookingDto>.Ok(
            result, ResponseMessages.BookingFetched));
    }

    // ── PUT /api/bookings/{id}/confirm ────────────────────────────────────
    [HttpPut("{id:guid}/confirm")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    public async Task<IActionResult> Confirm(
        Guid id, CancellationToken ct)
    {
        var result = await _service.ConfirmAsync(id, GetUserId(), ct);
        return Ok(ApiResponse<BookingDto>.Ok(
            result, ResponseMessages.BookingConfirmed));
    }

    // ── PUT /api/bookings/{id}/start ──────────────────────────────────────
    [HttpPut("{id:guid}/start")]
    [Authorize(Policy = AppRoles.StaffOrAdmin)]
    public async Task<IActionResult> Start(
        Guid id, CancellationToken ct)
    {
        var result = await _service.StartAsync(id, GetUserId(), ct);
        return Ok(ApiResponse<BookingDto>.Ok(
            result, ResponseMessages.BookingStarted));
    }

    // ── PUT /api/bookings/{id}/complete ───────────────────────────────────
    [HttpPut("{id:guid}/complete")]
    [Authorize(Policy = AppRoles.StaffOrAdmin)]
    public async Task<IActionResult> Complete(
        Guid id, CancellationToken ct)
    {
        var result = await _service.CompleteAsync(id, GetUserId(), ct);
        return Ok(ApiResponse<BookingDto>.Ok(
            result, ResponseMessages.BookingCompleted));
    }

    // ── POST /api/bookings/{id}/cancel ────────────────────────────────────
    [HttpPost("{id:guid}/cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromBody] CancelBookingRequest request,
        CancellationToken ct)
    {
        var isAdmin = User.IsInRole(AppRoles.Admin);
        var result = await _service.CancelAsync(
            id, GetUserId(), isAdmin, request, ct);

        return Ok(ApiResponse<BookingDto>.Ok(
            result, ResponseMessages.BookingCancelled));
    }

    // ── POST /api/bookings/{id}/reschedule ────────────────────────────────
    [HttpPost("{id:guid}/reschedule")]
    [Authorize(Policy = AppRoles.CustomerOnly)]
    public async Task<IActionResult> Reschedule(
        Guid id,
        [FromBody] RescheduleBookingRequest request,
        CancellationToken ct)
    {
        var v = await _rescheduleV.ValidateAsync(request, ct);
        if (!v.IsValid)
            return UnprocessableEntity(
                ApiResponse<object>.Fail("Validation failed.",
                    v.Errors.Select(e => e.ErrorMessage)));

        var result = await _service.RescheduleAsync(
            id, GetUserId(), request, ct);

        return Ok(ApiResponse<BookingDto>.Ok(
            result, ResponseMessages.BookingRescheduled));
    }

    // ── POST /api/bookings/{id}/payments ──────────────────────────────────
    [HttpPost("{id:guid}/payments")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    public async Task<IActionResult> RecordPayment(
        Guid id,
        [FromBody] RecordPaymentRequest request,
        CancellationToken ct)
    {
        var v = await _paymentV.ValidateAsync(request, ct);
        if (!v.IsValid)
            return UnprocessableEntity(
                ApiResponse<object>.Fail("Validation failed.",
                    v.Errors.Select(e => e.ErrorMessage)));

        var result = await _service.RecordPaymentAsync(
            id, GetUserId(), request, ct);

        return StatusCode(201,
            ApiResponse<PaymentDto>.Ok(result, ResponseMessages.PaymentRecorded));
    }

    // ── GET /api/bookings/{id}/payments ───────────────────────────────────
    [HttpGet("{id:guid}/payments")]
    [Authorize]
    public async Task<IActionResult> GetPayments(
        Guid id, CancellationToken ct)
    {
        var result = await _service.GetPaymentsAsync(id, ct);
        return Ok(ApiResponse<IReadOnlyList<PaymentDto>>.Ok(
            result, ResponseMessages.PaymentsFetched));
    }

    // ── PUT /api/bookings/{id}/consultation/schedule ──────────────────────
    [HttpPut("{id:guid}/consultation/schedule")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    public async Task<IActionResult> ScheduleConsultation(
        Guid id,
        [FromBody] ScheduleConsultationRequest request,
        CancellationToken ct)
    {
        var result = await _service.ScheduleConsultationAsync(
            id, GetUserId(), request, ct);

        return Ok(ApiResponse<BookingDto>.Ok(
            result, ResponseMessages.ConsultationScheduled));
    }

    // ── PUT /api/bookings/{id}/consultation/complete ──────────────────────
    [HttpPut("{id:guid}/consultation/complete")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    public async Task<IActionResult> CompleteConsultation(
        Guid id, CancellationToken ct)
    {
        var result = await _service.CompleteConsultationAsync(
            id, GetUserId(), ct);

        return Ok(ApiResponse<BookingDto>.Ok(
            result, ResponseMessages.ConsultationCompleted));
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}