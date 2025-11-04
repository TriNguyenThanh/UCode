using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssignmentService.Domain.Entities;

namespace AssignmentService.Infrastructure.EF.Configurations;

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        
        builder.HasKey(l => l.LanguageId);
        builder.Property(l => l.LanguageId)
            .ValueGeneratedOnAdd();
        
        builder.Property(l => l.Code)
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(l => l.DisplayName)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(l => l.DefaultTimeFactor)
            .HasColumnType("decimal(5,2)")
            .HasDefaultValue(1.0m)
            .IsRequired();

        builder.Property(l => l.DefaultMemoryKb);
        
        builder.Property(l => l.DefaultHead)
            .HasColumnType("NVARCHAR(MAX)");
        
        builder.Property(l => l.DefaultBody)
            .HasColumnType("NVARCHAR(MAX)");
        
        builder.Property(l => l.DefaultTail)
            .HasColumnType("NVARCHAR(MAX)");
        
        builder.Property(l => l.IsEnabled)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(l => l.DisplayOrder);

        builder.Property(l => l.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();
        
        // Indexes
        builder.HasIndex(l => l.Code)
            .IsUnique()
            .HasDatabaseName("ix_language_code");
        
        builder.HasIndex(l => l.DisplayOrder)
            .HasDatabaseName("ix_language_display_order");
        
        // ❌ REMOVED: ProblemLanguages relationship - đã config ở ProblemLanguageConfiguration
    }
}
