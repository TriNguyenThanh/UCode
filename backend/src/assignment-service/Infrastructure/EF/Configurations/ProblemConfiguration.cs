using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssignmentService.Domain.Entities;

namespace AssignmentService.Infrastructure.EF.Configurations;

/// <summary>
/// Configuration cho entity Problem (đã gộp ProblemVersion)
/// </summary>
public class ProblemConfiguration : IEntityTypeConfiguration<Problem>
{
    public void Configure(EntityTypeBuilder<Problem> builder)
    {
        /* ===== Primary Key ===== */
        builder.HasKey(p => p.ProblemId);
        
        /* ===== Columns Configuration ===== */
        
        // Code: Mã đề bài (unique)
        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50);
        
        // Slug: URL-friendly (unique)
        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(255);

        // Title: Tiêu đề
        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.OwnerId)
            .IsRequired();
    
        /* ===== Enum Properties =====
         * HasConversion<string>() convert enum → string trong DB
         * VD: Difficulty.EASY → "EASY"
         */
        builder.Property(p => p.Difficulty)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(16);
        
        builder.Property(p => p.Visibility)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(16);
        
        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(16);

        /* ===== Properties từ ProblemVersion ===== */
        
        builder.Property(p => p.Statement)
            .HasColumnType("NVARCHAR(MAX)");

        builder.Property(p => p.Solution)
            .HasColumnType("NVARCHAR(MAX)");

        builder.Property(p => p.IoMode)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(8)
            .HasDefaultValue(Domain.Enums.IoMode.STDIO);

        builder.Property(p => p.InputFormat)
            .HasMaxLength(1000);

        builder.Property(p => p.OutputFormat)
            .HasMaxLength(1000);

        builder.Property(p => p.Constraints)
            .HasMaxLength(2000);

        builder.Property(p => p.MaxScore);

        // Limits
        builder.Property(p => p.TimeLimitMs)
            .IsRequired()
            .HasDefaultValue(1000);

        builder.Property(p => p.MemoryLimitKb)
            .IsRequired()
            .HasDefaultValue(262144); // 256 MB

        builder.Property(p => p.SourceLimitKb)
            .IsRequired()
            .HasDefaultValue(65536); // 64 KB

        builder.Property(p => p.StackLimitKb)
            .IsRequired()
            .HasDefaultValue(8192); // 8 MB

        builder.Property(p => p.ValidatorRef)
            .HasMaxLength(1000);

        builder.Property(p => p.Changelog)
            .HasMaxLength(2000);

        builder.Property(p => p.IsLocked)
            .IsRequired()
            .HasDefaultValue(false);

        /* ===== Additional Fields for Desktop App ===== */
        
        builder.Property(p => p.Description)
            .HasColumnType("NVARCHAR(MAX)");
        
        builder.Property(p => p.SampleInput)
            .HasColumnType("NVARCHAR(MAX)");
        
        builder.Property(p => p.SampleOutput)
            .HasColumnType("NVARCHAR(MAX)");

        /* ===== Timestamp Properties =====
         * HasDefaultValueSql() set default value trong database
         */
        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()")
            .ValueGeneratedOnAdd() // Chỉ tạo giá trị khi thêm mới
            .ValueGeneratedOnAddOrUpdate();


        builder.Property(p => p.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");
        
        /* ===== Indexes =====
         * Tạo indexes cho performance
         */

        // Unique indexes
        builder.HasIndex(p => p.Code)
            .IsUnique()
            .HasDatabaseName("ix_problems_code");
        
        builder.HasIndex(p => p.Slug)
            .IsUnique()
            .HasDatabaseName("ix_problems_slug");
        
        // Single column indexes
        builder.HasIndex(p => p.OwnerId)
            .HasDatabaseName("ix_problems_ownerid");
        
        builder.HasIndex(p => p.Difficulty)
            .HasDatabaseName("ix_problems_difficulty");
        
        builder.HasIndex(p => p.Title)
            .HasDatabaseName("ix_problems_title");
        
        builder.HasIndex(p => p.UpdatedAt)
            .HasDatabaseName("ix_problems_updatedat");
        
        builder.HasIndex(p => p.IsLocked)
            .HasDatabaseName("ix_problems_islocked");

        // Composite index
        builder.HasIndex(p => new { p.Status, p.Visibility })
            .HasDatabaseName("ix_problems_status_visibility");
        
        /* ===== Relationships =====
         * ❌ REMOVED: All relationships đã được config ở phía dependent/junction table
         * - Datasets → DatasetConfiguration
         * - ProblemAssets → ProblemAssetConfiguration  
         * - ProblemLanguages → ProblemLanguageConfiguration
         * - ProblemTags → ProblemTagConfiguration
         */
    }
}