using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.DTOs.Staff;

public sealed record CreateStaffRequest(
    Guid UserId,
    string FullName,
    string PhoneNumber,
    string? AlternatePhone,
    string? ProfileImageUrl,
    string Designation,
    string? Bio,
    int ExperienceYears,
    Gender Gender,
    bool IsAvailableForOnSite,
    DateOnly JoinedAt
);