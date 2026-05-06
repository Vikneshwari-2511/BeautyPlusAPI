using BeautyPlusParlour.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautyPlusParlour.Data.Configurations;

public sealed class PaymentConfiguration
    : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .HasColumnType("decimal(10,2)");

        builder.Property(p => p.PaymentType)
            .HasConversion<string>().HasMaxLength(20);

        builder.Property(p => p.PaymentMethod)
            .HasConversion<string>().HasMaxLength(20);

        builder.Property(p => p.Status)
            .HasConversion<string>().HasMaxLength(20);

        builder.Property(p => p.TransactionId)
            .HasMaxLength(100);

        builder.HasIndex(p => p.BookingId);
    }
}