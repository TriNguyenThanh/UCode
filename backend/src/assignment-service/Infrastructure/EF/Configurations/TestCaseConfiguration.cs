using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssignmentService.Domain.Entities;

namespace AssignmentService.Infrastructure.EF.Configurations;

public class TestCaseConfiguration : IEntityTypeConfiguration<TestCase>
{
    public void Configure(EntityTypeBuilder<TestCase> builder)
    {
        builder.HasKey(t => t.TestCaseId);
        
        builder.Property(t => t.IndexNo)
            .IsRequired();
        
        builder.Property(t => t.InputRef)
            .IsRequired()
            .HasColumnType("NVARCHAR(MAX)");
        
        builder.Property(t => t.OutputRef)
            .IsRequired()
            .HasColumnType("NVARCHAR(MAX)");
        
        builder.Property(t => t.Score)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue("100");
        
        /* ===== Unique Constraint ===== */
        builder.HasIndex(t => new { t.DatasetId, t.IndexNo })
            .IsUnique();
        
        /* ===== Relationships ===== */
        builder.HasOne(t => t.Dataset)
            .WithMany(d => d.TestCases)
            .HasForeignKey(t => t.DatasetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}