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

        builder.Property(s => s.CompareResult)
            .HasMaxLength(255);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(s => s.ErrorCode)
            .HasMaxLength(50);

        builder.Property(s => s.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(s => s.SubmittedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSDATETIME()");

        // Relationships
        builder.HasOne(s => s.Assignment)
            .WithMany()
            .HasForeignKey(s => s.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Problem)
            .WithMany()
            .HasForeignKey(s => s.ProblemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Dataset)
            .WithMany()
            .HasForeignKey(s => s.DatasetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Language)
            .WithMany()
            .HasForeignKey(s => s.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.AssignmentId);
        builder.HasIndex(s => s.ProblemId);
        builder.HasIndex(s => s.DatasetId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.SubmittedAt);
    }
}