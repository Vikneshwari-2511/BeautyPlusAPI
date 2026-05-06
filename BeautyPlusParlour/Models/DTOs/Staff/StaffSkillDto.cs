using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.DTOs.Staff;

public sealed record AddSkillRequest(
    Guid ServiceId,
    ProficiencyLevel ProficiencyLevel
);

public sealed record StaffSkillDto(
    Guid Id,
    Guid ServiceId,
    string ServiceName,
    string CategoryName,
    ProficiencyLevel ProficiencyLevel,
    bool IsActive,
    DateTime CreatedAt
);