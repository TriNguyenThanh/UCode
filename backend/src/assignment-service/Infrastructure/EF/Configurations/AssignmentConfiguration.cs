using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssignmentService.Domain.Entities;

namespace AssignmentService.Infrastructure.EF.Configurations;

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.HasKey(a => a.AssignmentId);

        builder.Property(a => a.AssignmentType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(Domain.Enums.AssignmentStatus.DRAFT);

        builder.Property(a => a.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Description)
            .HasMaxLength(2000);

        builder.Property(a => a.ClassId)
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.AllowLateSubmission)
            .HasDefaultValue(false);

        builder.Property(a => a.StartTime)
            .IsRequired(false);

        builder.Property(a => a.EndTime)
            .IsRequired(false);

        builder.Property(a => a.AssignedBy)
            .IsRequired();

        // Relationships
        // builder.HasOne(a => a.AssignedByUser)
        //     .WithMany()
        //     .HasForeignKey(a => a.AssignedBy)
        //     .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(a => a.AssignedBy);
        builder.HasIndex(a => a.ClassId);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.CreatedAt);
    }
}