using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Data;
using BeautyPlusParlour.Exceptions;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Staff;
using BeautyPlusParlour.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeautyPlusParlour.Services;

public sealed class StaffScheduleService : IStaffScheduleService
{
    private readonly AppDbContext _db;

    public StaffScheduleService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<StaffScheduleDto>> GetAsync(
        Guid staffId, CancellationToken ct = default)
    {
        var exists = await _db.StaffProfiles
            .AnyAsync(s => s.Id == staffId, ct);

        if (!exists)
            throw new NotFoundException(ResponseMessages.StaffNotFound);

        var schedules = await _db.StaffSchedules
            .AsNoTracking()
            .Where(s => s.StaffId == staffId)
            .OrderBy(s => s.DayOfWeek)
            .ToListAsync(ct);

        return schedules.Select(MapToDto).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<StaffScheduleDto>> UpdateAsync(
        Guid staffId,
        IEnumerable<UpdateScheduleItemRequest> schedule,
        Guid updatedBy, CancellationToken ct = default)
    {
        var exists = await _db.StaffProfiles
            .AnyAsync(s => s.Id == staffId, ct);

        if (!exists)
            throw new NotFoundException(ResponseMessages.StaffNotFound);

        var items = schedule.ToList();

        // Validate all 7 days covered
        var days = items.Select(i => i.DayOfWeek).Distinct().ToList();
        if (days.Count != 7 || days.Any(d => d < 0 || d > 6))
            throw new AppException("Schedule must include all 7 days (0=Sunday to 6=Saturday).");

        // Validate working day times
        foreach (var item in items.Where(i => i.IsWorkingDay))
        {
            if (item.EndTime <= item.StartTime)
                throw new AppException(
                    $"End time must be after start time for {(DayOfWeek)item.DayOfWeek}.");
        }

        var existing = await _db.StaffSchedules
            .Where(s => s.StaffId == staffId)
            .ToListAsync(ct);

        foreach (var item in items)
        {
            var row = existing.FirstOrDefault(e => e.DayOfWeek == item.DayOfWeek);

            if (row is not null)
                row.Update(item.StartTime, item.EndTime, item.IsWorkingDay);
            else
                _db.StaffSchedules.Add(StaffSchedule.Create(
                    staffId, item.DayOfWeek,
                    item.StartTime, item.EndTime,
                    item.IsWorkingDay));
        }

        await _db.SaveChangesAsync(ct);
        return await GetAsync(staffId, ct);
    }

    private static StaffScheduleDto MapToDto(StaffSchedule s) =>
        new(s.Id, s.StaffId, s.DayOfWeek,
            ((DayOfWeek)s.DayOfWeek).ToString(),
            s.StartTime, s.EndTime, s.IsWorkingDay);
}