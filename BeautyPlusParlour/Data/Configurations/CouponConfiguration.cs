using BeautyPlusParlour.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautyPlusParlour.Data.Configurations;

public sealed class CouponConfiguration
    : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
            .HasMaxLength(50).IsRequired();
        builder.HasIndex(c => c.Code).IsUnique();

        builder.Property(c => c.Description)
            .HasMaxLength(200).IsRequired();

        builder.Property(c => c.CouponType)
            .HasConversion<string>()
            .HasMaxLength(20).IsRequired();

        builder.Property(c => c.Value)
            .HasColumnType("decimal(10,2)");

        builder.Property(c => c.MinOrderAmount)
            .HasColumnType("decimal(10,2)");

        builder.Property(c => c.MaxDiscount)
            .HasColumnType("decimal(10,2)");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Usages)
            .WithOne(u => u.Coupon)
            .HasForeignKey(u => u.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.IsActive);
        builder.HasIndex(c => new { c.ValidFrom, c.ValidTo });
    }
}