using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssignmentService.EF.Entities;

namespace AssignmentService.Infrastructure.EF.Configurations;

public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.HasKey(s => s.SubmissionId);

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(Domain.Enums.SubmissionStatus.Pending);

        builder.Property(a => a.AssignmentStudentId)
            .IsRequired();

        builder.Property(a => a.SubmittedAt)
            .IsRequired();

        // Relationships
        builder.HasOne<AssignmentProblemSubmission>()
            .WithMany(s => s.Submissions)
            .HasForeignKey(s => s.AssignmentId);

        // Indexes
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.AssignmentId);
        builder.HasIndex(a => a.ProblemId);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.SubmittedAt);
    }
}