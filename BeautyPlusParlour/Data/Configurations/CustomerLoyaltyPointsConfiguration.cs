using BeautyPlusParlour.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautyPlusParlour.Data.Configurations;

public sealed class CustomerLoyaltyPointsConfiguration
    : IEntityTypeConfiguration<CustomerLoyaltyPoints>
{
    public void Configure(EntityTypeBuilder<CustomerLoyaltyPoints> builder)
    {
        builder.HasKey(l => l.Id);

        builder.HasIndex(l => l.CustomerId).IsUnique();

        builder.Property(l => l.Tier)
            .HasConversion<string>()
            .HasMaxLength(20).IsRequired();

        builder.HasOne(l => l.Customer)
            .WithMany()
            .HasForeignKey(l => l.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(l => l.Tier);
    }
}