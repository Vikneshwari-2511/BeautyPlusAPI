using System.Text.Json;
using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Data;
using BeautyPlusParlour.Exceptions;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Staff;
using BeautyPlusParlour.Models.Entities;
using BeautyPlusParlour.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BeautyPlusParlour.Services;

public sealed class StaffService : IStaffService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly ILogger<StaffService> _logger;

    public StaffService(
        AppDbContext db,
        IAuditService audit,
        ILogger<StaffService> logger)
    {
        _db = db;
        _audit = audit;
        _logger = logger;
    }

    public async Task<StaffDto> CreateAsync(
        CreateStaffRequest request,
        Guid adminId, CancellationToken ct = default)
    {
        // Validate user exists and has Staff role
        var user = await _db.Users
            .FirstOrDefaultAsync(u =>
                u.Id == request.UserId && u.IsActive, ct)
            ?? throw new NotFoundException(ResponseMessages.StaffUserNotFound);

        if (user.Role != UserRole.Staff)
            throw new AppException("User must have Staff role to create a staff profile.");

        // Check no duplicate profile
        var exists = await _db.StaffProfiles
            .AnyAsync(s => s.UserId == request.UserId, ct);

        if (exists)
            throw new AppException(ResponseMessages.StaffUserAlreadyExists);

        var code = await GenerateEmployeeCodeAsync(ct);

        var staff = StaffProfile.Create(
            request.UserId, code,
            request.FullName, request.PhoneNumber,
            request.AlternatePhone, request.ProfileImageUrl,
            request.Designation, request.Bio,
            request.ExperienceYears, request.Gender,
            request.IsAvailableForOnSite, request.JoinedAt,
            adminId);

        _db.StaffProfiles.Add(staff);

        // Auto-create default schedule
        var defaultSchedule = StaffSchedule.CreateDefaultSchedule(staff.Id);
        _db.StaffSchedules.AddRange(defaultSchedule);

        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            adminId, AuditAction.Created,
            "StaffProfile", staff.Id.ToString(),
            null, JsonSerializer.Serialize(new
            {
                staff.EmployeeCode,
                staff.FullName,
                staff.Designation
            }), ct: ct);

        _logger.LogInformation(
            "Staff created: {Code} — {Name} by AdminId {AdminId}",
            staff.EmployeeCode, staff.FullName, adminId);

        return await GetByIdAsync(staff.Id, ct);
    }

    public async Task<StaffDto> UpdateAsync(
        Guid staffId, UpdateStaffRequest request,
        Guid adminId, CancellationToken ct = default)
    {
        var staff = await _db.StaffProfiles
            .FirstOrDefaultAsync(s => s.Id == staffId, ct)
            ?? throw new NotFoundException(ResponseMessages.StaffNotFound);

        var oldValues = JsonSerializer.Serialize(new
        {
            staff.FullName,
            staff.Designation
        });

        staff.Update(
            request.FullName, request.PhoneNumber,
            request.AlternatePhone, request.ProfileImageUrl,
            request.Designation, request.Bio,
            request.ExperienceYears, request.Gender,
            request.IsAvailableForOnSite, request.JoinedAt,
            adminId);

        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            adminId, AuditAction.Updated,
            "StaffProfile", staffId.ToString(),
            oldValues,
            JsonSerializer.Serialize(new { request.FullName }),
            ct: ct);

        return await GetByIdAsync(staffId, ct);
    }

    public async Task<StaffDto> UpdateOwnProfileAsync(
        Guid userId, UpdateOwnProfileRequest request,
        CancellationToken ct = default)
    {
        var staff = await _db.StaffProfiles
            .FirstOrDefaultAsync(s => s.UserId == userId, ct)
            ?? throw new NotFoundException(ResponseMessages.StaffNotFound);

        staff.Update(
            staff.FullName, request.PhoneNumber,
            request.AlternatePhone, request.ProfileImageUrl,
            staff.Designation, request.Bio,
            staff.ExperienceYears, staff.Gender,
            staff.IsAvailableForOnSite, staff.JoinedAt,
            userId);

        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(staff.Id, ct);
    }

    public async Task DeleteAsync(
        Guid staffId, Guid adminId,
        CancellationToken ct = default)
    {
        var staff = await _db.StaffProfiles
            .FirstOrDefaultAsync(s => s.Id == staffId, ct)
            ?? throw new NotFoundException(ResponseMessages.StaffNotFound);

        staff.SoftDelete(adminId);
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            adminId, AuditAction.Deleted,
            "StaffProfile", staffId.ToString(),
            ct: ct);
    }

    public async Task<StaffDto> GetByIdAsync(
        Guid staffId, CancellationToken ct = default)
    {
        var staff = await _db.StaffProfiles
            .AsNoTracking()
            .Include(s => s.Skills.Where(sk => sk.IsActive))
                .ThenInclude(sk => sk.Service)
                    .ThenInclude(svc => svc.Category)
            .Include(s => s.Schedules.OrderBy(sc => sc.DayOfWeek))
            .FirstOrDefaultAsync(s => s.Id == staffId, ct)
            ?? throw new NotFoundException(ResponseMessages.StaffNotFound);

        return MapToDto(staff);
    }

    public async Task<StaffDto> GetByUserIdAsync(
        Guid userId, CancellationToken ct = default)
    {
        var staff = await _db.StaffProfiles
            .AsNoTracking()
            .Include(s => s.Skills.Where(sk => sk.IsActive))
                .ThenInclude(sk => sk.Service)
                    .ThenInclude(svc => svc.Category)
            .Include(s => s.Schedules.OrderBy(sc => sc.DayOfWeek))
            .FirstOrDefaultAsync(s => s.UserId == userId, ct)
            ?? throw new NotFoundException(ResponseMessages.StaffNotFound);

        return MapToDto(staff);
    }

    public async Task<IReadOnlyList<StaffListDto>> GetAllAsync(
        bool includeInactive, CancellationToken ct = default)
    {
        var query = _db.StaffProfiles.AsNoTracking();

        if (!includeInactive)
            query = query.Where(s => s.IsActive);

        var list = await query
            .OrderBy(s => s.FullName)
            .ToListAsync(ct);

        var result = new List<StaffListDto>();

        foreach (var s in list)
        {
            var skillCount = await _db.StaffSkills
                .CountAsync(sk => sk.StaffId == s.Id && sk.IsActive, ct);

            result.Add(new StaffListDto(
                s.Id, s.EmployeeCode, s.FullName,
                s.ProfileImageUrl, s.Designation,
                s.ExperienceYears, s.Gender,
                s.IsAvailableForOnSite, s.IsActive,
                skillCount));
        }

        return result.AsReadOnly();
    }

    public async Task<IReadOnlyList<StaffAvailabilityDto>> GetAvailableForServiceAsync(
        Guid serviceId, DateOnly date, TimeOnly time,
        CancellationToken ct = default)
    {
        var service = await _db.Services
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.IsActive, ct)
            ?? throw new NotFoundException(ResponseMessages.ServiceNotFound);

        var totalSlot = service.DurationMinutes + service.BufferMinutes;
        var endTime = time.AddMinutes(totalSlot);
        var dayOfWeek = (int)date.DayOfWeek;

        // Get staff who have this skill
        var skilledStaffIds = await _db.StaffSkills
            .Where(sk => sk.ServiceId == serviceId && sk.IsActive)
            .Select(sk => sk.StaffId)
            .ToListAsync(ct);

        var result = new List<StaffAvailabilityDto>();

        foreach (var staffId in skilledStaffIds)
        {
            // Check schedule — is staff working this day during this time?
            var schedule = await _db.StaffSchedules
                .FirstOrDefaultAsync(sc =>
                    sc.StaffId == staffId &&
                    sc.DayOfWeek == dayOfWeek &&
                    sc.IsWorkingDay, ct);

            if (schedule is null) continue;
            if (time < schedule.StartTime || endTime > schedule.EndTime) continue;

            // Check approved leaves
            var onLeave = await _db.StaffLeaves
                .AnyAsync(l =>
                    l.StaffId == staffId &&
                    l.Status == LeaveStatus.Approved &&
                    l.LeaveFromDate <= date &&
                    l.LeaveToDate >= date, ct);

            if (onLeave) continue;

            var staff = await _db.StaffProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == staffId && s.IsActive, ct);

            if (staff is null) continue;

            var skill = await _db.StaffSkills
                .FirstOrDefaultAsync(sk =>
                    sk.StaffId == staffId &&
                    sk.ServiceId == serviceId, ct);

            result.Add(new StaffAvailabilityDto(
                staff.Id, staff.FullName,
                staff.ProfileImageUrl, staff.Designation,
                skill!.ProficiencyLevel.ToString(),
                staff.ExperienceYears,
                staff.IsAvailableForOnSite));
        }

        // Sort by proficiency level descending (Expert first)
        return result
            .OrderByDescending(s => s.ProficiencyLevel)
            .ThenByDescending(s => s.ExperienceYears)
            .ToList()
            .AsReadOnly();
    }

    // ── private ──────────────────────────────────────────────────────────
    private async Task<string> GenerateEmployeeCodeAsync(CancellationToken ct)
    {
        var count = await _db.StaffProfiles.CountAsync(ct);
        var code = $"{StaffConstants.EmployeeCodePrefix}-{(count + 1):D3}";

        // Ensure uniqueness
        while (await _db.StaffProfiles.AnyAsync(s => s.EmployeeCode == code, ct))
            code = $"{StaffConstants.EmployeeCodePrefix}-{(++count):D3}";

        return code;
    }

    private static StaffDto MapToDto(StaffProfile s) =>
        new(s.Id, s.UserId, s.EmployeeCode,
            s.FullName, s.PhoneNumber, s.AlternatePhone,
            s.ProfileImageUrl, s.Designation, s.Bio,
            s.ExperienceYears, s.Gender,
            s.IsAvailableForOnSite, s.IsActive, s.JoinedAt,
            s.CreatedAt,
            s.Skills.Select(sk => new StaffSkillDto(
                sk.Id, sk.ServiceId, sk.Service.Name,
                sk.Service.Category.Name,
                sk.ProficiencyLevel, sk.IsActive,
                sk.CreatedAt)).ToList().AsReadOnly(),
            s.Schedules.Select(sc => new StaffScheduleDto(
                sc.Id, sc.StaffId, sc.DayOfWeek,
                ((DayOfWeek)sc.DayOfWeek).ToString(),
                sc.StartTime, sc.EndTime,
                sc.IsWorkingDay)).ToList().AsReadOnly());
}