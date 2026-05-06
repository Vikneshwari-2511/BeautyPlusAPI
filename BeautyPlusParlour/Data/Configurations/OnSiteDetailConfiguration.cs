using BeautyPlusParlour.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautyPlusParlour.Data.Configurations;

public sealed class OnSiteDetailConfiguration : IEntityTypeConfiguration<OnSiteDetail>
{
    public void Configure(EntityTypeBuilder<OnSiteDetail> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.TravelCharge)
            .HasColumnType("decimal(10,2)").IsRequired();

        builder.Property(o => o.SpecialNotes)
            .HasColumnType("text");

        builder.HasIndex(o => o.ServiceId).IsUnique();
    }
}