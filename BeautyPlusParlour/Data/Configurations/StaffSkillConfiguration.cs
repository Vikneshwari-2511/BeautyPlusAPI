using BeautyPlusParlour.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautyPlusParlour.Data.Configurations;

public sealed class StaffSkillConfiguration
    : IEntityTypeConfiguration<StaffSkill>
{
    public void Configure(EntityTypeBuilder<StaffSkill> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.ProficiencyLevel)
            .HasConversion<string>()
            .HasMaxLength(20).IsRequired();

        // One staff cannot have same service twice
        builder.HasIndex(s => new { s.StaffId, s.ServiceId })
            .IsUnique();

        builder.HasOne(s => s.Service)
            .WithMany()
            .HasForeignKey(s => s.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}