using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssignmentService.Domain.Entities;

namespace AssignmentService.Infrastructure.EF.Configurations;

public class BestSubmissionConfiguration : IEntityTypeConfiguration<BestSubmission>
{
    public void Configure(EntityTypeBuilder<BestSubmission> builder)
    {
        // Map tới VIEW thay vì TABLE
        builder.ToView("best_submissions");

        // Đánh dấu là keyless vì view không có primary key
        builder.HasNoKey();

        // Map enum Status sang string
        builder.Property(bs => bs.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(s => s.UserFullName)
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(100);
        // VIEW không có relationships, indexes, hoặc constraints
        // Đây là read-only computed view
    }
}
