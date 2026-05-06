using BeautyPlusParlour.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautyPlusParlour.Data.Configurations;

public sealed class CustomerProfileConfiguration
    : IEntityTypeConfiguration<CustomerProfile>
{
    public void Configure(EntityTypeBuilder<CustomerProfile> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.FullName)
            .HasMaxLength(150).IsRequired();

        builder.Property(c => c.PhoneNumber)
            .HasMaxLength(20).IsRequired();

        builder.Property(c => c.ProfileImageUrl)
            .HasMaxLength(500);

        builder.Property(c => c.Gender)
            .HasConversion<string>()
            .HasMaxLength(20).IsRequired();

        // 1-to-1 with User
        builder.HasIndex(c => c.UserId).IsUnique();

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Addresses)
            .WithOne(a => a.Customer)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Favourites)
            .WithOne(f => f.Customer)
            .HasForeignKey(f => f.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.IsActive);
    }
}