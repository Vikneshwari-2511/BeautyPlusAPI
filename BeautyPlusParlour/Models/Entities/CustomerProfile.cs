using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.Entities;

public sealed class CustomerProfile
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public DateOnly? DateOfBirth { get; private set; }
    public Gender Gender { get; private set; }
    public string? ProfileImageUrl { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    // Navigation
    public User User { get; private set; } = null!;
    public ICollection<CustomerAddress> Addresses { get; private set; } = [];
    public ICollection<FavouriteService> Favourites { get; private set; } = [];

    private CustomerProfile() { }

    public static CustomerProfile Create(
        Guid userId, string fullName,
        string phoneNumber, Gender gender)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);

        return new CustomerProfile
        {
            UserId = userId,
            FullName = fullName.Trim(),
            PhoneNumber = phoneNumber.Trim(),
            Gender = gender
        };
    }

    public void Update(
        string fullName, string phoneNumber,
        DateOnly? dateOfBirth, Gender gender,
        string? profileImageUrl)
    {
        FullName = fullName.Trim();
        PhoneNumber = phoneNumber.Trim();
        DateOfBirth = dateOfBirth;
        Gender = gender;
        ProfileImageUrl = profileImageUrl?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate() => IsActive = false;
    public void Reactivate() => IsActive = true;
}