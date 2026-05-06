using BeautyPlusParlour.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautyPlusParlour.Data.Configurations;

public sealed class StaffScheduleConfiguration
    : IEntityTypeConfiguration<StaffSchedule>
{
    public void Configure(EntityTypeBuilder<StaffSchedule> builder)
    {
        builder.HasKey(s => s.Id);

        // One row per staff per day
        builder.HasIndex(s => new { s.StaffId, s.DayOfWeek })
            .IsUnique();

        builder.Property(s => s.DayOfWeek).IsRequired();
        builder.Property(s => s.IsWorkingDay).IsRequired();
    }
}