using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssignmentService.EF.Entities;

namespace AssignmentService.Infrastructure.EF.Configurations;

/// <summary>
/// Configuration cho entity User
/// Định nghĩa cách map User class → users table
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        /* ===== Table Name =====
         * Không cần ToTable() vì UseSnakeCaseNamingConvention() 
         * đã tự động convert User → users
         */
        
        /* ===== Primary Key =====
         * HasKey() định nghĩa primary key
         * EF Core tự nhận UserId là PK (vì tên kết thúc bằng Id)
         * Nhưng khai báo rõ ràng cho chắc chắn
         */
        builder.HasKey(u => u.UserId);
        
        /* ===== Column Configurations =====
         * Property() để configure từng column
         */
        
        // Email column
        builder.Property(u => u.Email)
            .IsRequired()                // NOT NULL
            .HasMaxLength(320);          // NVARCHAR(320)
            
        /* ===== Indexes =====
         * Tạo index để tăng tốc queries
         */
        builder.HasIndex(u => u.Email)
            .IsUnique();                 // Unique constraint
    }
}