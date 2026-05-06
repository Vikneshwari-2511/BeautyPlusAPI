using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.Entities;

public sealed class StaffSkill
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid StaffId { get; private set; }
    public Guid ServiceId { get; private set; }
    public ProficiencyLevel ProficiencyLevel { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation
    public StaffProfile Staff { get; private set; } = null!;
    public Service Service { get; private set; } = null!;

    private StaffSkill() { }

    public static StaffSkill Create(
        Guid staffId, Guid serviceId,
        ProficiencyLevel level, Guid createdBy)
    {
        return new StaffSkill
        {
            StaffId = staffId,
            ServiceId = serviceId,
            ProficiencyLevel = level,
            CreatedBy = createdBy
        };
    }

    public void Deactivate() => IsActive = false;

    public void UpdateLevel(ProficiencyLevel level) =>
        ProficiencyLevel = level;
}