using BeautyPlusParlour.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautyPlusParlour.Data.Configurations;

public sealed class SubCategoryConfiguration : IEntityTypeConfiguration<SubCategory>
{
    public void Configure(EntityTypeBuilder<SubCategory> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .HasMaxLength(100).IsRequired();

        builder.Property(s => s.Slug)
            .HasMaxLength(120).IsRequired();

        // Slug unique globally
        builder.HasIndex(s => s.Slug).IsUnique();

        // Name unique within same category
        builder.HasIndex(s => new { s.CategoryId, s.Name }).IsUnique();

        builder.Property(s => s.DisplayOrder)
            .HasDefaultValue(0);
    }
}