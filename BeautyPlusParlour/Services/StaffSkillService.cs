using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Data;
using BeautyPlusParlour.Exceptions;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Staff;
using BeautyPlusParlour.Models.Entities;
using BeautyPlusParlour.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BeautyPlusParlour.Services;

public sealed class StaffSkillService : IStaffSkillService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public StaffSkillService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<StaffSkillDto> AddAsync(
        Guid staffId, AddSkillRequest request,
        Guid adminId, CancellationToken ct = default)
    {
        var staffExists = await _db.StaffProfiles
            .AnyAsync(s => s.Id == staffId && s.IsActive, ct);

        if (!staffExists)
            throw new NotFoundException(ResponseMessages.StaffNotFound);

        var serviceExists = await _db.Services
            .AnyAsync(s => s.Id == request.ServiceId && s.IsActive, ct);

        if (!serviceExists)
            throw new NotFoundException(ResponseMessages.ServiceNotFound);

        var exists = await _db.StaffSkills
            .AnyAsync(sk =>
                sk.StaffId == staffId &&
                sk.ServiceId == request.ServiceId &&
                sk.IsActive, ct);

        if (exists)
            throw new AppException(ResponseMessages.SkillAlreadyExists);

        var skill = StaffSkill.Create(
            staffId, request.ServiceId,
            request.ProficiencyLevel, adminId);

        _db.StaffSkills.Add(skill);
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            adminId, AuditAction.Created,
            "StaffSkill", skill.Id.ToString(),
            null, null, ct: ct);

        return await GetSkillDtoAsync(skill.Id, ct);
    }

    public async Task RemoveAsync(
        Guid staffId, Guid skillId, Guid adminId,
        CancellationToken ct = default)
    {
        var skill = await _db.StaffSkills
            .FirstOrDefaultAsync(sk =>
                sk.Id == skillId &&
                sk.StaffId == staffId, ct)
            ?? throw new NotFoundException(ResponseMessages.SkillNotFound);

        skill.Deactivate();
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            adminId, AuditAction.Deleted,
            "StaffSkill", skillId.ToString(),
            ct: ct);
    }

    public async Task<IReadOnlyList<StaffSkillDto>> GetByStaffIdAsync(
        Guid staffId, CancellationToken ct = default)
    {
        var skills = await _db.StaffSkills
            .AsNoTracking()
            .Include(sk => sk.Service)
                .ThenInclude(s => s.Category)
            .Where(sk => sk.StaffId == staffId && sk.IsActive)
            .OrderBy(sk => sk.Service.Category.Name)
            .ThenBy(sk => sk.Service.Name)
            .ToListAsync(ct);

        return skills.Select(sk => new StaffSkillDto(
            sk.Id, sk.ServiceId, sk.Service.Name,
            sk.Service.Category.Name,
            sk.ProficiencyLevel, sk.IsActive,
            sk.CreatedAt))
            .ToList().AsReadOnly();
    }

    private async Task<StaffSkillDto> GetSkillDtoAsync(
        Guid skillId, CancellationToken ct)
    {
        var sk = await _db.StaffSkills
            .AsNoTracking()
            .Include(x => x.Service)
                .ThenInclude(s => s.Category)
            .FirstAsync(x => x.Id == skillId, ct);

        return new StaffSkillDto(
            sk.Id, sk.ServiceId, sk.Service.Name,
            sk.Service.Category.Name,
            sk.ProficiencyLevel, sk.IsActive,
            sk.CreatedAt);
    }
}