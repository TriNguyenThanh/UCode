using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssignmentService.Domain.Entities;
using AssignmentService.Domain.Enums;

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

        builder.Property(s => s.UserFullName)
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(100);

        builder.Property(s => s.SourceCodeRef)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.CompareResult)
            .HasMaxLength(255);

        builder.Property(s => s.Status)
            .HasConversion<string>();
    }
}
