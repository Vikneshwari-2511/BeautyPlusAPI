using BeautyPlusParlour.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautyPlusParlour.Data.Configurations;

public sealed class StaffLeaveConfiguration
    : IEntityTypeConfiguration<StaffLeave>
{
    public void Configure(EntityTypeBuilder<StaffLeave> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.LeaveType)
            .HasConversion<string>()
            .HasMaxLength(20).IsRequired();

        builder.Property(l => l.Status)
            .HasConversion<string>()
            .HasMaxLength(20).IsRequired();

        builder.Property(l => l.Reason)
            .HasMaxLength(500);

        builder.Property(l => l.RejectionReason)
            .HasMaxLength(500);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(l => l.ReviewedBy)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasIndex(l => new { l.StaffId, l.Status });
        builder.HasIndex(l => new { l.LeaveFromDate, l.LeaveToDate });
    }
}