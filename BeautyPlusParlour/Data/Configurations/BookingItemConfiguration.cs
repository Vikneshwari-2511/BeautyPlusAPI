using BeautyPlusParlour.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautyPlusParlour.Data.Configurations;

public sealed class BookingItemConfiguration
    : IEntityTypeConfiguration<BookingItem>
{
    public void Configure(EntityTypeBuilder<BookingItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ServiceName)
            .HasMaxLength(150).IsRequired();

        builder.Property(i => i.Price)
            .HasColumnType("decimal(10,2)");

        builder.HasOne(i => i.Service)
            .WithMany()
            .HasForeignKey(i => i.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}