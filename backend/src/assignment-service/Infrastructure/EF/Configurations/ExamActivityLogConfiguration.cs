using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssignmentService.Domain.Entities;

namespace AssignmentService.Infrastructure.EF.Configurations;

public class ExamActivityLogConfiguration : IEntityTypeConfiguration<ExamActivityLog>
{
    public void Configure(EntityTypeBuilder<ExamActivityLog> builder)
    {

        builder.HasKey(e => e.ActivityLogId);

        builder.Property(e => e.ActivityLogId)
            .IsRequired();

        builder.Property(e => e.AssignmentUserId)
            .IsRequired();

        builder.Property(e => e.ActivityType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Timestamp)
            .IsRequired();

        builder.Property(e => e.Metadata)
            .HasColumnType("text");

        builder.Property(e => e.SuspicionLevel)
            .HasDefaultValue(0);

        // Relationships
        builder.HasOne(e => e.AssignmentUser)
            .WithMany()
            .HasForeignKey(e => e.AssignmentUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.AssignmentUserId)
            .HasDatabaseName("idx_exam_activity_logs_assignment_user_id");

        builder.HasIndex(e => e.Timestamp)
            .HasDatabaseName("idx_exam_activity_logs_timestamp");

        builder.HasIndex(e => new { e.AssignmentUserId, e.Timestamp })
            .HasDatabaseName("idx_exam_activity_logs_user_timestamp");
    }
}
