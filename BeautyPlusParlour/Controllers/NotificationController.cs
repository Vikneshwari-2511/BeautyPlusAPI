using System.Security.Claims;
using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Common;
using BeautyPlusParlour.Models.DTOs.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeautyPlusParlour.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public sealed class NotificationController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationController(INotificationService service) =>
        _service = service;

    // ── GET /api/notifications ────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NotificationDto>>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(GetUserId(), ct);
        return Ok(ApiResponse<IReadOnlyList<NotificationDto>>.Ok(
            result, ResponseMessages.NotificationsFetched));
    }

    // ── GET /api/notifications/unread-count ───────────────────────────────
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(ApiResponse<UnreadCountDto>), 200)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
    {
        var result = await _service.GetUnreadCountAsync(GetUserId(), ct);
        return Ok(ApiResponse<UnreadCountDto>.Ok(result));
    }

    // ── PUT /api/notifications/{id}/read ──────────────────────────────────
    [HttpPut("{id:guid}/read")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> MarkRead(
        Guid id, CancellationToken ct)
    {
        await _service.MarkReadAsync(id, GetUserId(), ct);
        return Ok(ApiResponse<object>.Ok(
            null!, ResponseMessages.NotificationMarkedRead));
    }

    // ── PUT /api/notifications/read-all ───────────────────────────────────
    [HttpPut("read-all")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        await _service.MarkAllReadAsync(GetUserId(), ct);
        return Ok(ApiResponse<object>.Ok(
            null!, ResponseMessages.AllNotificationsRead));
    }

    // ── DELETE /api/notifications/{id} ────────────────────────────────────
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Delete(
        Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, GetUserId(), ct);
        return Ok(ApiResponse<object>.Ok(
            null!, ResponseMessages.NotificationDeleted));
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}