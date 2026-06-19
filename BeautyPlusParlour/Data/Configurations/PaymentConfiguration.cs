// Data/Configurations/PaymentConfiguration.cs
using BeautyPlusParlour.Models.Entities;
using BeautyPlusParlour.Models.Enums;
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

        // Nullable enum → stored as string, nullable column
        builder.Property(p => p.PaymentType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired(false);

        builder.Property(p => p.PaymentMethod)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired(false);

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.TransactionId)
            .HasMaxLength(100);

        builder.Property(p => p.RazorpayOrderId)
            .HasMaxLength(100);

        builder.Property(p => p.RazorpayPaymentId)
            .HasMaxLength(100);

        builder.Property(p => p.Currency)
            .HasMaxLength(10)
            .HasDefaultValue("INR");

        builder.HasIndex(p => p.BookingId);
        builder.HasIndex(p => p.RazorpayOrderId);
    }
}