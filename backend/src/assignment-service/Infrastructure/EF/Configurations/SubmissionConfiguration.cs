using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssignmentService.Domain.Entities;

namespace AssignmentService.Infrastructure.EF.Configurations;

public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        // builder.ToTable("Submissions");
        
        builder.HasKey(s => s.SubmissionId);

        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.ProblemId)
            .IsRequired();

        builder.Property(s => s.DatasetId)
            .IsRequired();

        builder.Property(s => s.SourceCodeRef)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Language)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.CompareResult)
            .HasMaxLength(255);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.ErrorCode)
            .HasMaxLength(50);

        builder.Property(s => s.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(s => s.SubmittedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSDATETIME()");

        // Relationships
        builder.HasOne(s => s.AssignmentUser)
            .WithMany(au => au.Submissions)
            .HasForeignKey(s => s.AssignmentUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Problem)
            .WithMany()
            .HasForeignKey(s => s.ProblemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.AssignmentUserId);
        builder.HasIndex(s => s.ProblemId);
        builder.HasIndex(s => s.DatasetId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.SubmittedAt);
    }
}