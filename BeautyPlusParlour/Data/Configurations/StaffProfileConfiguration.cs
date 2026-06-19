using BeautyPlusParlour.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautyPlusParlour.Data.Configurations;

public sealed class StaffProfileConfiguration
    : IEntityTypeConfiguration<StaffProfile>
{
    public void Configure(EntityTypeBuilder<StaffProfile> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.EmployeeCode)
            .HasMaxLength(20).IsRequired();
        builder.HasIndex(s => s.EmployeeCode).IsUnique();

        builder.Property(s => s.UserId).IsRequired();
        builder.HasIndex(s => s.UserId).IsUnique();

        builder.Property(s => s.FullName)
            .HasMaxLength(150).IsRequired();

        builder.Property(s => s.PhoneNumber)
            .HasMaxLength(20).IsRequired();

        builder.Property(s => s.AlternatePhone)
            .HasMaxLength(20);

        builder.Property(s => s.ProfileImageUrl)
            .HasMaxLength(500);

        builder.Property(s => s.Designation)
    .HasConversion<string>()
    .HasMaxLength(50)
    .IsRequired();

        builder.Property(s => s.Bio)
            .HasColumnType("text");

        builder.Property(s => s.Gender)
            .HasConversion<string>()
            .HasMaxLength(20).IsRequired();

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Skills)
            .WithOne(sk => sk.Staff)
            .HasForeignKey(sk => sk.StaffId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Schedules)
            .WithOne(sc => sc.Staff)
            .HasForeignKey(sc => sc.StaffId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Leaves)
            .WithOne(l => l.Staff)
            .HasForeignKey(l => l.StaffId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.IsActive);
    }
}