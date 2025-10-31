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

        // Relationships
        builder.HasOne(bs => bs.AssignmentUser)
            .WithMany(au => au.BestSubmissions)
            .HasForeignKey(bs => bs.AssignmentUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bs => bs.Problem)
            .WithMany()
            .HasForeignKey(bs => bs.ProblemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(bs => bs.Submission)
            .WithMany()
            .HasForeignKey(bs => bs.SubmissionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(bs => bs.AssignmentUserId);
        builder.HasIndex(bs => bs.ProblemId);
        builder.HasIndex(bs => bs.SubmissionId);
        builder.HasIndex(bs => new { bs.AssignmentUserId, bs.ProblemId })
            .IsUnique();
    }
}
