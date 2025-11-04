using AssignmentService.Domain.Entities;
using AssignmentService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssignmentService.Infrastructure.Data.Configurations;

public class ProblemAssetConfiguration : IEntityTypeConfiguration<ProblemAsset>
{
    public void Configure(EntityTypeBuilder<ProblemAsset> builder)
    {
        builder.HasKey(pa => pa.ProblemAssetId);
        
        builder.Property(pa => pa.ProblemAssetId)
            .ValueGeneratedOnAdd();
        
        builder.Property(pa => pa.ProblemId)
            .IsRequired();
        
        builder.Property(pa => pa.Type)
            .IsRequired()
            .HasMaxLength(16)
            .HasConversion<string>();
        
        builder.Property(pa => pa.ObjectRef)
            .IsRequired()
            .HasColumnType("nvarchar(max)");
        
        builder.Property(pa => pa.Checksum)
            .HasMaxLength(100);
        
        builder.Property(pa => pa.Title)
            .HasColumnType("nvarchar(max)");
        
        builder.Property(pa => pa.Format)
            .IsRequired()
            .HasMaxLength(16)
            .HasConversion<string>()
            .HasDefaultValue(ContentFormat.MARKDOWN);
        
        builder.Property(pa => pa.OrderIndex)
            .IsRequired()
            .HasDefaultValue(0);
        
        builder.Property(pa => pa.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(pa => pa.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");
        
        // Foreign Key
        builder.HasOne(pa => pa.Problem)
            .WithMany(p => p.ProblemAssets)
            .HasForeignKey(pa => pa.ProblemId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(pa => pa.ProblemId)
            .HasDatabaseName("ix_problemassets_problemid");
        
        builder.HasIndex(pa => new { pa.ProblemId, pa.Type, pa.OrderIndex })
            .HasDatabaseName("ix_problemassets_problemid_type_orderindex");
    }
}