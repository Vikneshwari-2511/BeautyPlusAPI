namespace BeautyPlusParlour.Models.DTOs.Staff;

public sealed record StaffScheduleDto(
    Guid Id,
    Guid StaffId,
    int DayOfWeek,
    string DayName,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsWorkingDay
);

public sealed record UpdateScheduleItemRequest(
    int DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsWorkingDay
);