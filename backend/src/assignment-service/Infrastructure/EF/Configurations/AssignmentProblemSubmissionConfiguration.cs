using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssignmentService.EF.Entities;

namespace AssignmentService.Infrastructure.EF.Configurations;

public class AssignmentProblemSubmissionConfiguration : IEntityTypeConfiguration<AssignmentProblemSubmission>
{
    public void Configure(EntityTypeBuilder<AssignmentProblemSubmission> builder)
    {
        builder.HasKey(aps => aps.SubmissionId);

        builder.Property(aps => aps.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(aps => aps.MaxScore)
            .IsRequired();

        builder.Property(aps => aps.SolutionCode)
            .HasMaxLength(10000);

        builder.Property(aps => aps.TeacherFeedback)
            .HasMaxLength(2000);

        builder.Property(aps => aps.AttemptCount)
            .HasDefaultValue(0);

        // Relationships
        builder.HasOne(aps => aps.AssignmentDetail)
            .WithMany()
            .HasForeignKey(aps => aps.AssignmentDetailId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(aps => aps.Problem)
            .WithMany()
            .HasForeignKey(aps => aps.ProblemId)
            .OnDelete(DeleteBehavior.Restrict);

        // AssignmentProblem relationship - removed to avoid cascade cycles
        // builder.HasOne(aps => aps.AssignmentProblem)
        //     .WithMany()
        //     .HasForeignKey(aps => new { aps.AssignmentDetailId, aps.ProblemId })
        //     .OnDelete(DeleteBehavior.NoAction);

        // Indexes
        builder.HasIndex(aps => aps.AssignmentDetailId);
        builder.HasIndex(aps => aps.ProblemId);
        builder.HasIndex(aps => new { aps.AssignmentDetailId, aps.ProblemId })
            .IsUnique();
    }
}
