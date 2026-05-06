using BeautyPlusParlour.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautyPlusParlour.Data.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(100).IsRequired();

        builder.Property(c => c.Slug)
            .HasMaxLength(120).IsRequired();

        builder.HasIndex(c => c.Slug).IsUnique();
        builder.HasIndex(c => c.Name).IsUnique();

        builder.Property(c => c.Description)
            .HasColumnType("text");

        builder.Property(c => c.ServiceTypeDefault)
            .HasConversion<string>()
            .HasMaxLength(20).IsRequired();

        builder.Property(c => c.ImageUrl)
            .HasMaxLength(500);

        builder.Property(c => c.DisplayOrder)
            .HasDefaultValue(0);

        // Audit FK — no cascade, just lookup
        builder.HasOne(c => c.CreatedByUser)
            .WithMany()
            .HasForeignKey(c => c.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.SubCategories)
            .WithOne(s => s.Category)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Services)
            .WithOne(s => s.Category)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}