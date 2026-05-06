using System.Security.Claims;
using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Common;
using BeautyPlusParlour.Models.DTOs.Staff;
using BeautyPlusParlour.Models.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeautyPlusParlour.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class StaffController : ControllerBase
{
    private readonly IStaffService _staff;
    private readonly IStaffSkillService _skills;
    private readonly IStaffScheduleService _schedules;
    private readonly IStaffLeaveService _leaves;
    private readonly IValidator<CreateStaffRequest> _createV;
    private readonly IValidator<UpdateStaffRequest> _updateV;
    private readonly IValidator<UpdateOwnProfileRequest> _ownV;
    private readonly IValidator<AddSkillRequest> _skillV;
    private readonly IValidator<RequestLeaveRequest> _leaveV;
    private readonly IValidator<RejectLeaveRequest> _rejectV;

    public StaffController(
        IStaffService staff,
        IStaffSkillService skills,
        IStaffScheduleService schedules,
        IStaffLeaveService leaves,
        IValidator<CreateStaffRequest> createV,
        IValidator<UpdateStaffRequest> updateV,
        IValidator<UpdateOwnProfileRequest> ownV,
        IValidator<AddSkillRequest> skillV,
        IValidator<RequestLeaveRequest> leaveV,
        IValidator<RejectLeaveRequest> rejectV)
    {
        _staff = staff;
        _skills = skills;
        _schedules = schedules;
        _leaves = leaves;
        _createV = createV;
        _updateV = updateV;
        _ownV = ownV;
        _skillV = skillV;
        _leaveV = leaveV;
        _rejectV = rejectV;
    }

    // ── STAFF PROFILE ─────────────────────────────────────────────────────

    [HttpPost]
    [Authorize(Policy = AppRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<StaffDto>), 201)]
    public async Task<IActionResult> Create(
        [FromBody] CreateStaffRequest request, CancellationToken ct)
    {
        var v = await _createV.ValidateAsync(request, ct);
        if (!v.IsValid)
            return UnprocessableEntity(
                ApiResponse<object>.Fail("Validation failed.",
                    v.Errors.Select(e => e.ErrorMessage)));

        var adminId = GetUserId();
        var result = await _staff.CreateAsync(request, adminId, ct);

        return StatusCode(201,
            ApiResponse<StaffDto>.Ok(result, ResponseMessages.StaffCreated));
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool includeInactive = false,
        CancellationToken ct = default)
    {
        var canSeeInactive = User.Identity?.IsAuthenticated == true
            && User.IsInRole(AppRoles.Admin);

        var result = await _staff.GetAllAsync(
            includeInactive && canSeeInactive, ct);

        return Ok(ApiResponse<IReadOnlyList<StaffListDto>>.Ok(
            result, ResponseMessages.StaffListFetched));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(
        Guid id, CancellationToken ct)
    {
        var result = await _staff.GetByIdAsync(id, ct);
        return Ok(ApiResponse<StaffDto>.Ok(result, ResponseMessages.StaffProfileFetched));
    }

    [HttpGet("my")]
    [Authorize(Policy = AppRoles.StaffOrAdmin)]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
    {
        var result = await _staff.GetByUserIdAsync(GetUserId(), ct);
        return Ok(ApiResponse<StaffDto>.Ok(result, ResponseMessages.StaffProfileFetched));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateStaffRequest request, CancellationToken ct)
    {
        var v = await _updateV.ValidateAsync(request, ct);
        if (!v.IsValid)
            return UnprocessableEntity(
                ApiResponse<object>.Fail("Validation failed.",
                    v.Errors.Select(e => e.ErrorMessage)));

        var result = await _staff.UpdateAsync(id, request, GetUserId(), ct);
        return Ok(ApiResponse<StaffDto>.Ok(result, ResponseMessages.StaffUpdated));
    }

    [HttpPut("my")]
    [Authorize(Policy = AppRoles.StaffOrAdmin)]
    public async Task<IActionResult> UpdateOwnProfile(
        [FromBody] UpdateOwnProfileRequest request, CancellationToken ct)
    {
        var v = await _ownV.ValidateAsync(request, ct);
        if (!v.IsValid)
            return UnprocessableEntity(
                ApiResponse<object>.Fail("Validation failed.",
                    v.Errors.Select(e => e.ErrorMessage)));

        var result = await _staff.UpdateOwnProfileAsync(GetUserId(), request, ct);
        return Ok(ApiResponse<StaffDto>.Ok(result, ResponseMessages.StaffUpdated));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _staff.DeleteAsync(id, GetUserId(), ct);
        return Ok(ApiResponse<object>.Ok(null!, ResponseMessages.StaffDeleted));
    }

    // ── AVAILABILITY ──────────────────────────────────────────────────────

    [HttpGet("available")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailable(
        [FromQuery] Guid serviceId,
        [FromQuery] DateOnly date,
        [FromQuery] TimeOnly time,
        CancellationToken ct)
    {
        var result = await _staff.GetAvailableForServiceAsync(
            serviceId, date, time, ct);

        return Ok(ApiResponse<IReadOnlyList<StaffAvailabilityDto>>.Ok(
            result, ResponseMessages.AvailableStaffFetched));
    }

    // ── SKILLS ────────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/skills")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    public async Task<IActionResult> AddSkill(
        Guid id, [FromBody] AddSkillRequest request, CancellationToken ct)
    {
        var v = await _skillV.ValidateAsync(request, ct);
        if (!v.IsValid)
            return UnprocessableEntity(
                ApiResponse<object>.Fail("Validation failed.",
                    v.Errors.Select(e => e.ErrorMessage)));

        var result = await _skills.AddAsync(id, request, GetUserId(), ct);
        return StatusCode(201,
            ApiResponse<StaffSkillDto>.Ok(result, ResponseMessages.SkillAdded));
    }

    [HttpGet("{id:guid}/skills")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSkills(
        Guid id, CancellationToken ct)
    {
        var result = await _skills.GetByStaffIdAsync(id, ct);
        return Ok(ApiResponse<IReadOnlyList<StaffSkillDto>>.Ok(
            result, ResponseMessages.SkillsFetched));
    }

    [HttpDelete("{id:guid}/skills/{skillId:guid}")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    public async Task<IActionResult> RemoveSkill(
        Guid id, Guid skillId, CancellationToken ct)
    {
        await _skills.RemoveAsync(id, skillId, GetUserId(), ct);
        return Ok(ApiResponse<object>.Ok(null!, ResponseMessages.SkillRemoved));
    }

    // ── SCHEDULE ─────────────────────────────────────────────────────────

    [HttpGet("{id:guid}/schedule")]
    [Authorize(Policy = AppRoles.StaffOrAdmin)]
    public async Task<IActionResult> GetSchedule(
        Guid id, CancellationToken ct)
    {
        var result = await _schedules.GetAsync(id, ct);
        return Ok(ApiResponse<IReadOnlyList<StaffScheduleDto>>.Ok(
            result, ResponseMessages.ScheduleFetched));
    }

    [HttpPut("{id:guid}/schedule")]
    [Authorize(Policy = AppRoles.StaffOrAdmin)]
    public async Task<IActionResult> UpdateSchedule(
        Guid id,
        [FromBody] List<UpdateScheduleItemRequest> request,
        CancellationToken ct)
    {
        var result = await _schedules.UpdateAsync(id, request, GetUserId(), ct);
        return Ok(ApiResponse<IReadOnlyList<StaffScheduleDto>>.Ok(
            result, ResponseMessages.ScheduleUpdated));
    }

    // ── LEAVES ────────────────────────────────────────────────────────────

    [HttpPost("my/leave")]
    [Authorize(Policy = AppRoles.StaffOrAdmin)]
    public async Task<IActionResult> RequestLeave(
        [FromBody] RequestLeaveRequest request, CancellationToken ct)
    {
        var v = await _leaveV.ValidateAsync(request, ct);
        if (!v.IsValid)
            return UnprocessableEntity(
                ApiResponse<object>.Fail("Validation failed.",
                    v.Errors.Select(e => e.ErrorMessage)));

        var result = await _leaves.RequestLeaveAsync(GetUserId(), request, ct);
        return StatusCode(201,
            ApiResponse<StaffLeaveDto>.Ok(result, ResponseMessages.LeaveRequested));
    }

    [HttpGet("my/leaves")]
    [Authorize(Policy = AppRoles.StaffOrAdmin)]
    public async Task<IActionResult> GetMyLeaves(CancellationToken ct)
    {
        var result = await _leaves.GetMyLeavesAsync(GetUserId(), ct);
        return Ok(ApiResponse<IReadOnlyList<StaffLeaveDto>>.Ok(result));
    }

    [HttpGet("leaves/pending")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    public async Task<IActionResult> GetPendingLeaves(CancellationToken ct)
    {
        var result = await _leaves.GetPendingAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<StaffLeaveDto>>.Ok(result));
    }

    [HttpGet("leaves")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    public async Task<IActionResult> GetAllLeaves(
        [FromQuery] Guid? staffId,
        [FromQuery] LeaveStatus? status,
        CancellationToken ct)
    {
        var result = await _leaves.GetAllAsync(staffId, status, ct);
        return Ok(ApiResponse<IReadOnlyList<StaffLeaveDto>>.Ok(result));
    }

    [HttpPut("leaves/{leaveId:guid}/approve")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    public async Task<IActionResult> ApproveLeave(
        Guid leaveId, CancellationToken ct)
    {
        var result = await _leaves.ApproveAsync(leaveId, GetUserId(), ct);
        return Ok(ApiResponse<StaffLeaveDto>.Ok(result, ResponseMessages.LeaveApproved));
    }

    [HttpPut("leaves/{leaveId:guid}/reject")]
    [Authorize(Policy = AppRoles.AdminOnly)]
    public async Task<IActionResult> RejectLeave(
        Guid leaveId,
        [FromBody] RejectLeaveRequest request,
        CancellationToken ct)
    {
        var v = await _rejectV.ValidateAsync(request, ct);
        if (!v.IsValid)
            return UnprocessableEntity(
                ApiResponse<object>.Fail("Validation failed.",
                    v.Errors.Select(e => e.ErrorMessage)));

        var result = await _leaves.RejectAsync(leaveId, GetUserId(), request, ct);
        return Ok(ApiResponse<StaffLeaveDto>.Ok(result, ResponseMessages.LeaveRejected));
    }

    [HttpDelete("my/leave/{leaveId:guid}")]
    [Authorize(Policy = AppRoles.StaffOrAdmin)]
    public async Task<IActionResult> CancelLeave(
        Guid leaveId, CancellationToken ct)
    {
        await _leaves.CancelAsync(leaveId, GetUserId(), ct);
        return Ok(ApiResponse<object>.Ok(null!, ResponseMessages.LeaveCancelled));
    }

    // ── helper ────────────────────────────────────────────────────────────
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}