using BeautyPlusParlour.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautyPlusParlour.Data.Configurations;

public sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.RefreshTokenHash).IsRequired();
        builder.Property(s => s.ReplacedByTokenHash).IsRequired(false);
        builder.Property(s => s.DeviceInfo).HasMaxLength(500);
        builder.Property(s => s.Browser).HasMaxLength(200);
        builder.Property(s => s.IpAddress).HasMaxLength(50);
        builder.Property(s => s.Location).HasMaxLength(200);

        builder.HasIndex(s => s.RefreshTokenHash);
        builder.HasIndex(s => s.ReplacedByTokenHash);  // ← for breach lookup
        builder.HasIndex(s => new { s.UserId, s.IsRevoked });
    }
}