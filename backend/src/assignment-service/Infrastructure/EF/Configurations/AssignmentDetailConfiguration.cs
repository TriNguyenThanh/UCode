using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssignmentService.EF.Entities;

namespace AssignmentService.Infrastructure.EF.Configurations;

public class AssignmentDetailConfiguration : IEntityTypeConfiguration<AssignmentDetail>
{
    public void Configure(EntityTypeBuilder<AssignmentDetail> builder)
    {
        builder.HasKey(ad => ad.AssignmentDetailId);

        builder.Property(ad => ad.AssignmentDetailId)
            .ValueGeneratedOnAdd();

        builder.Property(ad => ad.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(ad => ad.AssignedAt)
            .IsRequired();

        // removed per-problem fields migrated to AssignmentProblemSubmission

        // Relationships
        builder.HasOne(ad => ad.Assignment)
            .WithMany(a => a.AssignmentDetails)
            .HasForeignKey(ad => ad.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(ad => ad.AssignmentId);
        builder.HasIndex(ad => ad.StudentId);
    }
}
