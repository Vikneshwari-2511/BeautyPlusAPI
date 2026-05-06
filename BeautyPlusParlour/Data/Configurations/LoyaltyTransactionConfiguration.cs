using BeautyPlusParlour.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautyPlusParlour.Data.Configurations;

public sealed class LoyaltyTransactionConfiguration
    : IEntityTypeConfiguration<LoyaltyTransaction>
{
    public void Configure(EntityTypeBuilder<LoyaltyTransaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TransactionType)
            .HasConversion<string>()
            .HasMaxLength(20).IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(200).IsRequired();

        builder.HasOne(t => t.Customer)
            .WithMany()
            .HasForeignKey(t => t.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Booking)
            .WithMany()
            .HasForeignKey(t => t.BookingId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasIndex(t => new { t.CustomerId, t.CreatedAt });
        builder.HasIndex(t => new { t.TransactionType, t.ExpiresAt });
    }
}