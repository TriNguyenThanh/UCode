using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssignmentService.EF.Entities;

namespace AssignmentService.Infrastructure.EF.Configurations;

public class CodeTemplateConfiguration : IEntityTypeConfiguration<CodeTemplate>
{
    public void Configure(EntityTypeBuilder<CodeTemplate> builder)
    {
        builder.HasKey(ct => ct.CodeTemplateId);

        builder.Property(ct => ct.Lang)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ct => ct.StarterRef)
            .IsRequired()
            .HasMaxLength(1000);

        // Index
        builder.HasIndex(ct => ct.ProblemId)
            .HasDatabaseName("ix_codetemplates_problemid");

        builder.HasIndex(ct => ct.Lang)
            .HasDatabaseName("ix_codetemplates_lang");

        // Relationship: CodeTemplate belongs to Problem (đã đổi từ ProblemVersion)
        builder.HasOne(ct => ct.Problem)
            .WithMany(p => p.CodeTemplates)
            .HasForeignKey(ct => ct.ProblemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}