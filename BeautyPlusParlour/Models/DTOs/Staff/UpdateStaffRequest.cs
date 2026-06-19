using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.DTOs.Staff;

public sealed record UpdateStaffRequest(
    string FullName,
    string PhoneNumber,
    string? AlternatePhone,
    string? ProfileImageUrl,
    Designation Designation,
    string? Bio,
    int ExperienceYears,
    Gender Gender,
    bool IsAvailableForOnSite,
    DateOnly JoinedAt
);