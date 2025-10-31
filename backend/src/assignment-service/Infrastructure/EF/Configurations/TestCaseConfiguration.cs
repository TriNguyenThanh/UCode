using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssignmentService.EF.Entities;

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
            .HasMaxLength(500);
        
        builder.Property(t => t.OutputRef)
            .IsRequired()
            .HasMaxLength(500);
        
        /* ===== Decimal Property =====
         * HasColumnType() chỉ định chính xác type trong DB
         * DECIMAL(5,2) = 5 digits total, 2 sau dấu phẩy
         * VD: 999.99
         */
        builder.Property(t => t.Weight)
            .IsRequired()
            .HasColumnType("DECIMAL(5,2)")
            .HasDefaultValue(1.00m);  // m suffix = decimal literal

        builder.Property(t => t.IsSample)
            .IsRequired()
            .HasDefaultValue(false);
        
        /* ===== Unique Constraint ===== */
        builder.HasIndex(t => new { t.DatasetId, t.IndexNo })
            .IsUnique();
        
        /* ===== Other Indexes ===== */
        builder.HasIndex(t => t.IsSample);
        builder.HasIndex(t => t.Weight);
        
        /* ===== Relationships ===== */
        builder.HasOne(t => t.Dataset)
            .WithMany(d => d.TestCases)
            .HasForeignKey(t => t.DatasetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}