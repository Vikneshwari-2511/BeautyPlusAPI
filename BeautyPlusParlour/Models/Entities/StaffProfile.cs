using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.Entities;

public sealed class StaffProfile
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string EmployeeCode { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string? AlternatePhone { get; private set; }
    public string? ProfileImageUrl { get; private set; }
    public string Designation { get; private set; } = string.Empty;
    public string? Bio { get; private set; }
    public int ExperienceYears { get; private set; }
    public Gender Gender { get; private set; }
    public bool IsAvailableForOnSite { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateOnly JoinedAt { get; private set; }

    // Audit
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public Guid? UpdatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    // Navigation
    public User User { get; private set; } = null!;
    public ICollection<StaffSkill> Skills { get; private set; } = [];
    public ICollection<StaffSchedule> Schedules { get; private set; } = [];
    public ICollection<StaffLeave> Leaves { get; private set; } = [];

    private StaffProfile() { }

    public static StaffProfile Create(
        Guid userId, string employeeCode,
        string fullName, string phoneNumber,
        string? alternatePhone, string? profileImageUrl,
        string designation, string? bio,
        int experienceYears, Gender gender,
        bool isAvailableForOnSite, DateOnly joinedAt,
        Guid createdBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(designation);

        return new StaffProfile
        {
            UserId = userId,
            EmployeeCode = employeeCode,
            FullName = fullName.Trim(),
            PhoneNumber = phoneNumber.Trim(),
            AlternatePhone = alternatePhone?.Trim(),
            ProfileImageUrl = profileImageUrl?.Trim(),
            Designation = designation.Trim(),
            Bio = bio?.Trim(),
            ExperienceYears = experienceYears,
            Gender = gender,
            IsAvailableForOnSite = isAvailableForOnSite,
            JoinedAt = joinedAt,
            CreatedBy = createdBy
        };
    }

    public void Update(
        string fullName, string phoneNumber,
        string? alternatePhone, string? profileImageUrl,
        string designation, string? bio,
        int experienceYears, Gender gender,
        bool isAvailableForOnSite, DateOnly joinedAt,
        Guid updatedBy)
    {
        FullName = fullName.Trim();
        PhoneNumber = phoneNumber.Trim();
        AlternatePhone = alternatePhone?.Trim();
        ProfileImageUrl = profileImageUrl?.Trim();
        Designation = designation.Trim();
        Bio = bio?.Trim();
        ExperienceYears = experienceYears;
        Gender = gender;
        IsAvailableForOnSite = isAvailableForOnSite;
        JoinedAt = joinedAt;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete(Guid deletedBy)
    {
        IsActive = false;
        DeletedBy = deletedBy;
        DeletedAt = DateTime.UtcNow;
    }

    public void Reactivate(Guid updatedBy)
    {
        IsActive = true;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}