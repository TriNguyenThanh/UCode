using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssignmentService.Domain.Entities;

namespace AssignmentService.Infrastructure.EF.Configurations;

public class ProblemAssetConfiguration : IEntityTypeConfiguration<ProblemAsset>
{
    public void Configure(EntityTypeBuilder<ProblemAsset> builder)
    {
        builder.HasKey(pa => pa.ProblemAssetId);

        builder.Property(pa => pa.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(16);

        builder.Property(pa => pa.ObjectRef)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(pa => pa.Checksum)
            .HasMaxLength(100);

        builder.Property(pa => pa.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSDATETIME()");

        // Indexes
        builder.HasIndex(pa => pa.ProblemId)
            .HasDatabaseName("ix_problemassets_problemid");

        builder.HasIndex(pa => pa.Type)
            .HasDatabaseName("ix_problemassets_type");

        // Relationship: ProblemAsset belongs to Problem (đã đổi từ ProblemVersion)
        builder.HasOne(pa => pa.Problem)
            .WithMany(p => p.ProblemAssets)
            .HasForeignKey(pa => pa.ProblemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}