using BeautyPlusParlour.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautyPlusParlour.Data.Configurations;

public sealed class FavouriteServiceConfiguration
    : IEntityTypeConfiguration<FavouriteService>
{
    public void Configure(EntityTypeBuilder<FavouriteService> builder)
    {
        builder.HasKey(f => f.Id);

        // One customer can't favourite same service twice
        builder.HasIndex(f => new { f.CustomerId, f.ServiceId })
            .IsUnique();

        builder.HasOne(f => f.Service)
            .WithMany()
            .HasForeignKey(f => f.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}