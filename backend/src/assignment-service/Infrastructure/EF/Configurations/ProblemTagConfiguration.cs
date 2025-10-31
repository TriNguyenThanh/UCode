using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssignmentService.EF.Entities;

namespace AssignmentService.Infrastructure.EF.Configurations;

/// <summary>
/// Configuration cho ProblemTag (Junction Table)
/// Đại diện cho Many-to-Many relationship giữa Problems và Tags
/// </summary>
public class ProblemTagConfiguration : IEntityTypeConfiguration<ProblemTag>
{
    public void Configure(EntityTypeBuilder<ProblemTag> builder)
    {
        /* ===== Composite Primary Key ===== */
        builder.HasKey(pt => new { pt.ProblemId, pt.TagId });
        
        /* ===== Indexes ===== */
        builder.HasIndex(pt => pt.TagId);
        
        /* ===== Relationships =====
         * Configure cả 2 chiều của Many-to-Many
         */
        
        // ProblemTag → Problem (Many-to-One)
        builder.HasOne(pt => pt.Problem)
            .WithMany(p => p.ProblemTags)
            .HasForeignKey(pt => pt.ProblemId)
            .OnDelete(DeleteBehavior.Cascade);  // Xóa Problem → xóa ProblemTags
        
        // ProblemTag → Tag (Many-to-One)
        builder.HasOne(pt => pt.Tag)
            .WithMany(t => t.ProblemTags)
            .HasForeignKey(pt => pt.TagId)
            .OnDelete(DeleteBehavior.Cascade);  // Xóa Tag → xóa ProblemTags
    }
}