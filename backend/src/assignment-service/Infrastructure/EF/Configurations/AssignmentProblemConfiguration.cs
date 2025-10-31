using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssignmentService.Domain.Entities;

namespace AssignmentService.Infrastructure.EF.Configurations;

public class AssignmentProblemConfiguration : IEntityTypeConfiguration<AssignmentProblem>
{
    public void Configure(EntityTypeBuilder<AssignmentProblem> builder)
    {
        // Composite Primary Key
        builder.HasKey(ap => new { ap.AssignmentId, ap.ProblemId });

        builder.Property(ap => ap.Points)
            .IsRequired()
            .HasDefaultValue(100);

        builder.Property(ap => ap.OrderIndex)
            .IsRequired();

        // Relationships
        builder.HasOne(ap => ap.Assignment)
            .WithMany(a => a.AssignmentProblems)
            .HasForeignKey(ap => ap.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ap => ap.Problem)
            .WithMany()
            .HasForeignKey(ap => ap.ProblemId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(ap => ap.AssignmentId);
        builder.HasIndex(ap => ap.ProblemId);
    }
}
