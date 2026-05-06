using BeautyPlusParlour.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautyPlusParlour.Data.Configurations;

public sealed class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .HasMaxLength(150).IsRequired();

        builder.Property(s => s.Slug)
            .HasMaxLength(180).IsRequired();

        builder.HasIndex(s => s.Slug).IsUnique();

        builder.Property(s => s.Description)
            .HasColumnType("text");

        builder.Property(s => s.ServiceTypeActual)
            .HasConversion<string>()
            .HasMaxLength(20).IsRequired();

        builder.Property(s => s.Gender)
            .HasConversion<string>()
            .HasMaxLength(20).IsRequired();

        builder.Property(s => s.BasePrice)
            .HasColumnType("decimal(10,2)").IsRequired();

        builder.Property(s => s.DiscountedPrice)
            .HasColumnType("decimal(10,2)");

        builder.Property(s => s.TaxPercent)
            .HasColumnType("decimal(5,2)");

        builder.Property(s => s.ImageUrl)
            .HasMaxLength(500);

        builder.Property(s => s.DisplayOrder)
            .HasDefaultValue(0);

        builder.Property(s => s.IsTaxInclusive)
            .HasDefaultValue(true);

        // SubCategoryId is nullable
        builder.HasOne(s => s.SubCategory)
            .WithMany(sc => sc.Services)
            .HasForeignKey(s => s.SubCategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(s => s.OnSiteDetail)
            .WithOne(o => o.Service)
            .HasForeignKey<OnSiteDetail>(o => o.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.CategoryId, s.IsActive });
        builder.HasIndex(s => new { s.ServiceTypeActual, s.IsActive });
        builder.HasIndex(s => new { s.Gender, s.IsActive });
        builder.HasIndex(s => s.IsFeatured);
        builder.HasIndex(s => s.IsPopular);
    }
}