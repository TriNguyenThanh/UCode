using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssignmentService.Domain.Entities;

namespace AssignmentService.Infrastructure.EF.Configurations;

public class DatasetConfiguration : IEntityTypeConfiguration<Dataset>
{
    public void Configure(EntityTypeBuilder<Dataset> builder)
    {
        builder.HasKey(d => d.DatasetId);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Kind)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(16);

        // Index
        builder.HasIndex(d => d.ProblemId)
            .HasDatabaseName("ix_datasets_problemid");

        // Relationship: Dataset belongs to Problem (đã đổi từ ProblemVersion)
        builder.HasOne(d => d.Problem)
            .WithMany(p => p.Datasets)
            .HasForeignKey(d => d.ProblemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship: Dataset has many TestCases
        builder.HasMany(d => d.TestCases)
            .WithOne(tc => tc.Dataset)
            .HasForeignKey(tc => tc.DatasetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}