using BeautyPlusParlour.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautyPlusParlour.Data.Configurations;

public sealed class BookingConfiguration
    : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.BookingCode)
            .HasMaxLength(20).IsRequired();
        builder.HasIndex(b => b.BookingCode).IsUnique();

        builder.Property(b => b.BookingType)
            .HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(b => b.Status)
            .HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(b => b.TotalAmount)
            .HasColumnType("decimal(10,2)");

        builder.Property(b => b.DiscountAmount)
            .HasColumnType("decimal(10,2)");

        builder.Property(b => b.TravelCharge)
            .HasColumnType("decimal(10,2)");

        builder.Property(b => b.FinalAmount)
            .HasColumnType("decimal(10,2)");

        builder.Property(b => b.AdvanceAmount)
            .HasColumnType("decimal(10,2)");

        builder.Property(b => b.Notes)
            .HasColumnType("text");

        builder.Property(b => b.CancellationReason)
            .HasMaxLength(500);

        builder.Property(b => b.CouponCode)
            .HasMaxLength(50);

        builder.HasOne(b => b.Customer)
            .WithMany()
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Staff)
            .WithMany()
            .HasForeignKey(b => b.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Address)
            .WithMany()
            .HasForeignKey(b => b.AddressId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasMany(b => b.Items)
            .WithOne(i => i.Booking)
            .HasForeignKey(i => i.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.Payments)
            .WithOne(p => p.Booking)
            .HasForeignKey(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(b => new { b.CustomerId, b.Status });
        builder.HasIndex(b => new { b.StaffId, b.BookingDate, b.Status });
        builder.HasIndex(b => new { b.BookingDate, b.Status });
    }
}