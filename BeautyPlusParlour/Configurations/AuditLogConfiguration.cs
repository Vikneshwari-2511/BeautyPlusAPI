using BeautyPlusParlour.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautyPlusParlour.Data.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Action).HasMaxLength(50).IsRequired();
        builder.Property(a => a.EntityName).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityId).HasMaxLength(100);
        builder.Property(a => a.IpAddress).HasMaxLength(50);
        builder.Property(a => a.OldValues).HasColumnType("text");
        builder.Property(a => a.NewValues).HasColumnType("text");
        builder.HasIndex(a => new { a.UserId, a.CreatedAt });
        builder.HasIndex(a => a.EntityName);
    }
}