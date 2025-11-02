using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssignmentService.Domain.Entities;

namespace AssignmentService.Infrastructure.EF.Configurations;

public class BestSubmissionConfiguration : IEntityTypeConfiguration<BestSubmission>
{
    public void Configure(EntityTypeBuilder<BestSubmission> builder)
    {
        // builder.ToTable("BestSubmissions");
        
        builder.HasKey(bs => bs.BestSubmissionId);

        // AssignmentUserId và ProblemId chỉ là stored fields (không có FK relationship)
        builder.Property(bs => bs.AssignmentUserId)
            .IsRequired();

        builder.Property(bs => bs.ProblemId)
            .IsRequired();

        builder.Property(bs => bs.Score)
            .IsRequired();

        builder.Property(bs => bs.MaxScore)
            .IsRequired();

        builder.Property(bs => bs.TotalTime)
            .IsRequired();

        builder.Property(bs => bs.TotalMemory)
            .IsRequired();

        builder.Property(bs => bs.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSDATETIME()");

        // Relationships - CHỈ có relationship với Submission
        builder.HasOne(bs => bs.Submission)
            .WithOne(s => s.BestSubmission)
            .HasForeignKey<BestSubmission>(bs => bs.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(bs => bs.AssignmentUserId);
        builder.HasIndex(bs => bs.ProblemId);
        builder.HasIndex(bs => bs.SubmissionId);
        
        // Unique constraint: một AssignmentUser chỉ có 1 best submission cho mỗi Problem
        builder.HasIndex(bs => new { bs.AssignmentUserId, bs.ProblemId })
            .IsUnique();
    }
}
