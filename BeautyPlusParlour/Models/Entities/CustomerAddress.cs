namespace BeautyPlusParlour.Models.Entities;

public sealed class CustomerAddress
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid CustomerId { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string AddressLine1 { get; private set; } = string.Empty;
    public string? AddressLine2 { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string PinCode { get; private set; } = string.Empty;
    public string? Landmark { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    // Navigation
    public CustomerProfile Customer { get; private set; } = null!;

    private CustomerAddress() { }

    public static CustomerAddress Create(
        Guid customerId, string label,
        string addressLine1, string? addressLine2,
        string city, string state, string pinCode,
        string? landmark, bool isDefault)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(addressLine1);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);

        return new CustomerAddress
        {
            CustomerId = customerId,
            Label = label.Trim(),
            AddressLine1 = addressLine1.Trim(),
            AddressLine2 = addressLine2?.Trim(),
            City = city.Trim(),
            State = state.Trim(),
            PinCode = pinCode.Trim(),
            Landmark = landmark?.Trim(),
            IsDefault = isDefault
        };
    }

    public void Update(
        string label, string addressLine1,
        string? addressLine2, string city,
        string state, string pinCode, string? landmark)
    {
        Label = label.Trim();
        AddressLine1 = addressLine1.Trim();
        AddressLine2 = addressLine2?.Trim();
        City = city.Trim();
        State = state.Trim();
        PinCode = pinCode.Trim();
        Landmark = landmark?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnsetDefault()
    {
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}