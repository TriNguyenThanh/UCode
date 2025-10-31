using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssignmentService.EF.Entities;

namespace AssignmentService.Infrastructure.EF.Configurations;

public class LanguageLimitConfiguration : IEntityTypeConfiguration<LanguageLimit>
{
    public void Configure(EntityTypeBuilder<LanguageLimit> builder)
    {
        builder.HasKey(ll => ll.LanguageLimitId);

        builder.Property(ll => ll.Lang)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ll => ll.TimeFactor)
            .HasPrecision(5, 2);

        // Indexes
        builder.HasIndex(ll => ll.ProblemId)
            .HasDatabaseName("ix_languagelimits_problemid");

        builder.HasIndex(ll => ll.Lang)
            .HasDatabaseName("ix_languagelimits_lang");

        // Composite index for lookups
        builder.HasIndex(ll => new { ll.ProblemId, ll.Lang })
            .IsUnique()
            .HasDatabaseName("ix_languagelimits_problemid_lang");

        // Relationship: LanguageLimit belongs to Problem (đã đổi từ ProblemVersion)
        builder.HasOne(ll => ll.Problem)
            .WithMany(p => p.LanguageLimits)
            .HasForeignKey(ll => ll.ProblemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}