namespace BeautyPlusParlour.Models.Entities;

public sealed class StaffSchedule
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid StaffId { get; private set; }
    public int DayOfWeek { get; private set; } // 0=Sun … 6=Sat
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public bool IsWorkingDay { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    // Navigation
    public StaffProfile Staff { get; private set; } = null!;

    private StaffSchedule() { }

    public static StaffSchedule Create(
        Guid staffId, int dayOfWeek,
        TimeOnly startTime, TimeOnly endTime,
        bool isWorkingDay)
    {
        return new StaffSchedule
        {
            StaffId = staffId,
            DayOfWeek = dayOfWeek,
            StartTime = startTime,
            EndTime = endTime,
            IsWorkingDay = isWorkingDay
        };
    }

    public void Update(
        TimeOnly startTime, TimeOnly endTime,
        bool isWorkingDay)
    {
        StartTime = startTime;
        EndTime = endTime;
        IsWorkingDay = isWorkingDay;
        UpdatedAt = DateTime.UtcNow;
    }

    // Default schedule factory
    public static List<StaffSchedule> CreateDefaultSchedule(Guid staffId)
    {
        var schedules = new List<StaffSchedule>();
        var defaultStart = new TimeOnly(9, 0);
        var defaultEnd = new TimeOnly(19, 0);

        for (int day = 0; day <= 6; day++)
        {
            schedules.Add(Create(
                staffId, day,
                day == 0 ? TimeOnly.MinValue : defaultStart,
                day == 0 ? TimeOnly.MinValue : defaultEnd,
                isWorkingDay: day != 0)); // Sunday = day off
        }

        return schedules;
    }
}