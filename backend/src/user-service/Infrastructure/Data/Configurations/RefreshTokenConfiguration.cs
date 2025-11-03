using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration cho RefreshToken entity
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(rt => rt.RefreshTokenId);

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(rt => rt.UserId)
            .IsRequired();

        builder.Property(rt => rt.CreatedAt)
            .IsRequired();

        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();

        builder.Property(rt => rt.LastUsedAt);

        builder.Property(rt => rt.CreatedByIp)
            .HasMaxLength(45);

        builder.Property(rt => rt.LastUsedByIp)
            .HasMaxLength(45);

        builder.Property(rt => rt.CreatedByUserAgent)
            .HasMaxLength(500);

        builder.Property(rt => rt.LastUsedByUserAgent)
            .HasMaxLength(500);

        builder.Property(rt => rt.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(rt => rt.RevokedReason)
            .HasMaxLength(200);

        builder.Property(rt => rt.RevokedAt);

        builder.Property(rt => rt.ReplacedByToken)
            .HasMaxLength(500);

        // Foreign key relationship
        builder.HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(rt => rt.Token)
            .IsUnique();

        builder.HasIndex(rt => rt.UserId);

        builder.HasIndex(rt => rt.ExpiresAt);

        builder.HasIndex(rt => new { rt.UserId, rt.Status });
    }
}
