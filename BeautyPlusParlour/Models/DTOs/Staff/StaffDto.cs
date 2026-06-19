using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.DTOs.Staff;

public sealed record StaffDto(
    Guid Id,
    Guid UserId,
    string EmployeeCode,
    string FullName,
    string PhoneNumber,
    string? AlternatePhone,
    string? ProfileImageUrl,
    Designation Designation,
    string? Bio,
    int ExperienceYears,
    Gender Gender,
    bool IsAvailableForOnSite,
    bool IsActive,
    DateOnly JoinedAt,
    DateTime CreatedAt,
    IReadOnlyList<StaffSkillDto> Skills,
    IReadOnlyList<StaffScheduleDto> Schedule
);

public sealed record StaffListDto(
    Guid Id,
    string EmployeeCode,
    string FullName,
    string? ProfileImageUrl,
    Designation Designation,
    int ExperienceYears,
    Gender Gender,
    bool IsAvailableForOnSite,
    bool IsActive,
    int SkillCount
);

public sealed record StaffAvailabilityDto(
    Guid StaffId,
    string FullName,
    string? ProfileImageUrl,
    Designation Designation,
    string ProficiencyLevel,
    int ExperienceYears,
    bool IsAvailableForOnSite
);