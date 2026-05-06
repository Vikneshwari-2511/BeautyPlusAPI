using BeautyPlusParlour.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautyPlusParlour.Data.Configurations;

public sealed class CustomerAddressConfiguration
    : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Label)
            .HasMaxLength(50).IsRequired();

        builder.Property(a => a.AddressLine1)
            .HasMaxLength(250).IsRequired();

        builder.Property(a => a.AddressLine2)
            .HasMaxLength(250);

        builder.Property(a => a.City)
            .HasMaxLength(100).IsRequired();

        builder.Property(a => a.State)
            .HasMaxLength(100).IsRequired();

        builder.Property(a => a.PinCode)
            .HasMaxLength(10).IsRequired();

        builder.Property(a => a.Landmark)
            .HasMaxLength(200);

        builder.HasIndex(a => new { a.CustomerId, a.IsDefault });
        builder.HasIndex(a => new { a.CustomerId, a.IsActive });
    }
}