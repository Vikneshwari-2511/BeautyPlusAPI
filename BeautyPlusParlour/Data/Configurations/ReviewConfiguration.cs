using BeautyPlusParlour.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautyPlusParlour.Data.Configurations;

public sealed class ReviewConfiguration
    : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);

        // One review per booking
        builder.HasIndex(r => r.BookingId).IsUnique();

        builder.Property(r => r.Comment)
            .HasColumnType("text");

        builder.Property(r => r.HideReason)
            .HasMaxLength(200);

        builder.HasOne(r => r.Booking)
            .WithMany()
            .HasForeignKey(r => r.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Staff)
            .WithMany()
            .HasForeignKey(r => r.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Service)
            .WithMany()
            .HasForeignKey(r => r.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(r => r.HiddenBy)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasIndex(r => new { r.ServiceId, r.IsVisible });
        builder.HasIndex(r => new { r.StaffId, r.IsVisible });
        builder.HasIndex(r => new { r.CustomerId, r.CreatedAt });
    }
}