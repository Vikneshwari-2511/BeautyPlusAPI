using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Data;
using BeautyPlusParlour.Exceptions;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Staff;
using BeautyPlusParlour.Models.Entities;
using BeautyPlusParlour.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BeautyPlusParlour.Services;

public sealed class StaffLeaveService : IStaffLeaveService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly INotificationService _notifications;
    public StaffLeaveService(AppDbContext db, IAuditService audit, INotificationService notifications)
    {
        _db = db;
        _audit = audit;
        _notifications = notifications;
    }

    public async Task<StaffLeaveDto> RequestLeaveAsync(
        Guid userId, RequestLeaveRequest request,
        CancellationToken ct = default)
    {
        var staff = await _db.StaffProfiles
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive, ct)
            ?? throw new NotFoundException(ResponseMessages.StaffNotFound);

        // Check for overlapping leave
        var overlap = await _db.StaffLeaves
            .AnyAsync(l =>
                l.StaffId == staff.Id &&
                l.Status != LeaveStatus.Rejected &&
                l.LeaveFromDate <= request.LeaveToDate &&
                l.LeaveToDate >= request.LeaveFromDate, ct);

        if (overlap)
            throw new AppException(ResponseMessages.LeaveDateConflict);

        var leave = StaffLeave.Create(
            staff.Id, request.LeaveType,
            request.LeaveFromDate, request.LeaveToDate,
            request.Reason);

        _db.StaffLeaves.Add(leave);
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            userId, AuditAction.Created,
            "StaffLeave", leave.Id.ToString(),
            null, null, ct: ct);

        return await GetLeaveDtoAsync(leave.Id, ct);
    }

    public async Task<StaffLeaveDto> ApproveAsync(
        Guid leaveId, Guid adminId,
        CancellationToken ct = default)
    {
        var leave = await _db.StaffLeaves
            .FirstOrDefaultAsync(l => l.Id == leaveId, ct)
            ?? throw new NotFoundException(ResponseMessages.LeaveNotFound);

        if (!leave.IsPending())
            throw new AppException(ResponseMessages.LeaveAlreadyReviewed);

        leave.Approve(adminId);
        await _db.SaveChangesAsync(ct);
        await _notifications.NotifyLeaveApprovedAsync(leaveId, ct);
        await _audit.LogAsync(
            adminId, AuditAction.Updated,
            "StaffLeave", leaveId.ToString(),
            null, "Approved", ct: ct);

        return await GetLeaveDtoAsync(leaveId, ct);
    }

    public async Task<StaffLeaveDto> RejectAsync(
        Guid leaveId, Guid adminId,
        RejectLeaveRequest request,
        CancellationToken ct = default)
    {
        var leave = await _db.StaffLeaves
            .FirstOrDefaultAsync(l => l.Id == leaveId, ct)
            ?? throw new NotFoundException(ResponseMessages.LeaveNotFound);

        if (!leave.IsPending())
            throw new AppException(ResponseMessages.LeaveAlreadyReviewed);

        leave.Reject(adminId, request.RejectionReason);
        await _db.SaveChangesAsync(ct);
        await _notifications.NotifyLeaveRejectedAsync(leaveId, ct);
        await _audit.LogAsync(
            adminId, AuditAction.Updated,
            "StaffLeave", leaveId.ToString(),
            null, "Rejected", ct: ct);

        return await GetLeaveDtoAsync(leaveId, ct);
    }

    public async Task CancelAsync(
        Guid leaveId, Guid userId,
        CancellationToken ct = default)
    {
        var staff = await _db.StaffProfiles
            .FirstOrDefaultAsync(s => s.UserId == userId, ct)
            ?? throw new NotFoundException(ResponseMessages.StaffNotFound);

        var leave = await _db.StaffLeaves
            .FirstOrDefaultAsync(l =>
                l.Id == leaveId &&
                l.StaffId == staff.Id, ct)
            ?? throw new NotFoundException(ResponseMessages.LeaveNotFound);

        if (!leave.IsPending())
            throw new AppException("Only pending leaves can be cancelled.");

        leave.Cancel();
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<StaffLeaveDto>> GetMyLeavesAsync(
        Guid userId, CancellationToken ct = default)
    {
        var staff = await _db.StaffProfiles
            .FirstOrDefaultAsync(s => s.UserId == userId, ct)
            ?? throw new NotFoundException(ResponseMessages.StaffNotFound);

        var leaves = await _db.StaffLeaves
            .AsNoTracking()
            .Include(l => l.Staff)
            .Where(l => l.StaffId == staff.Id)
            .OrderByDescending(l => l.RequestedAt)
            .ToListAsync(ct);

        return await MapLeavesAsync(leaves, ct);
    }

    public async Task<IReadOnlyList<StaffLeaveDto>> GetPendingAsync(
        CancellationToken ct = default)
    {
        var leaves = await _db.StaffLeaves
            .AsNoTracking()
            .Include(l => l.Staff)
            .Where(l => l.Status == LeaveStatus.Pending)
            .OrderBy(l => l.LeaveFromDate)
            .ToListAsync(ct);

        return await MapLeavesAsync(leaves, ct);
    }

    public async Task<IReadOnlyList<StaffLeaveDto>> GetAllAsync(
        Guid? staffId, LeaveStatus? status,
        CancellationToken ct = default)
    {
        var query = _db.StaffLeaves
            .AsNoTracking()
            .Include(l => l.Staff)
            .AsQueryable();

        if (staffId.HasValue)
            query = query.Where(l => l.StaffId == staffId);

        if (status.HasValue)
            query = query.Where(l => l.Status == status);

        var leaves = await query
            .OrderByDescending(l => l.RequestedAt)
            .ToListAsync(ct);

        return await MapLeavesAsync(leaves, ct);
    }

    // ── private ──────────────────────────────────────────────────────────
    private async Task<StaffLeaveDto> GetLeaveDtoAsync(
        Guid leaveId, CancellationToken ct)
    {
        var leave = await _db.StaffLeaves
            .AsNoTracking()
            .Include(l => l.Staff)
            .FirstAsync(l => l.Id == leaveId, ct);

        return await MapLeaveAsync(leave, ct);
    }

    private async Task<IReadOnlyList<StaffLeaveDto>> MapLeavesAsync(
        IEnumerable<StaffLeave> leaves, CancellationToken ct)
    {
        var result = new List<StaffLeaveDto>();
        foreach (var l in leaves)
            result.Add(await MapLeaveAsync(l, ct));
        return result.AsReadOnly();
    }

    private async Task<StaffLeaveDto> MapLeaveAsync(
        StaffLeave l, CancellationToken ct)
    {
        string? reviewerName = null;

        if (l.ReviewedBy.HasValue)
        {
            var reviewer = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == l.ReviewedBy, ct);
            reviewerName = reviewer?.FullName;
        }

        return new StaffLeaveDto(
            l.Id, l.StaffId, l.Staff.FullName,
            l.Staff.EmployeeCode, l.LeaveType,
            l.LeaveFromDate, l.LeaveToDate,
            l.TotalDays, l.Reason, l.Status,
            l.RequestedAt, reviewerName,
            l.ReviewedAt, l.RejectionReason);
    }
}