using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssignmentService.Domain.Entities;

namespace AssignmentService.Infrastructure.EF.Configurations;

public class ProblemLanguageConfiguration : IEntityTypeConfiguration<ProblemLanguage>
{
    public void Configure(EntityTypeBuilder<ProblemLanguage> builder)
    {
        // Composite Primary Key (ProblemId, LanguageId)
        builder.HasKey(pl => new { pl.ProblemId, pl.LanguageId });
        
        builder.Property(pl => pl.ProblemId)
            .IsRequired();
        
        builder.Property(pl => pl.LanguageId)
            .IsRequired();
        
        builder.Property(pl => pl.TimeFactorOverride)
            .HasColumnType("decimal(5,2)");

        builder.Property(pl => pl.MemoryKbOverride);
        
        builder.Property(pl => pl.HeadOverride)
            .HasColumnType("NVARCHAR(MAX)");
        
        builder.Property(pl => pl.BodyOverride)
            .HasColumnType("NVARCHAR(MAX)");
        
        builder.Property(pl => pl.TailOverride)
            .HasColumnType("NVARCHAR(MAX)");
        
        builder.Property(pl => pl.IsAllowed)
            .HasDefaultValue(true)
            .IsRequired();
        
        builder.Property(pl => pl.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();
        
        // Note: Composite PK already creates unique constraint
        // No need for separate unique index on (ProblemId, LanguageId)
        
        builder.HasIndex(pl => pl.ProblemId)
            .HasDatabaseName("ix_problem_language_problem");
        
        builder.HasIndex(pl => pl.LanguageId)
            .HasDatabaseName("ix_problem_language_language");
        
        // Relationships
        builder.HasOne(pl => pl.Problem)
            .WithMany(p => p.ProblemLanguages)
            .HasForeignKey(pl => pl.ProblemId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_problem_language_problem");
        
        builder.HasOne(pl => pl.Language)
            .WithMany(l => l.ProblemLanguages)
            .HasForeignKey(pl => pl.LanguageId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_problem_language_language");
    }
}
