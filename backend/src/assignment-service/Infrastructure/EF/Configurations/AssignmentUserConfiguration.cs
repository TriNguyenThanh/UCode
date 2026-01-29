using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssignmentService.Domain.Entities;

namespace AssignmentService.Infrastructure.EF.Configurations;

public class AssignmentUserConfiguration : IEntityTypeConfiguration<AssignmentUser>
{
    public void Configure(EntityTypeBuilder<AssignmentUser> builder)
    {
        // builder.ToTable("AssignmentUsers");

        builder.HasKey(au => au.AssignmentUserId);

        builder.Property(au => au.AssignmentUserId)
            .ValueGeneratedOnAdd();

        builder.Property(au => au.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(au => au.AssignedAt)
            .IsRequired();

        builder.Property(au => au.UserId)
            .IsRequired();

        builder.Property(au => au.TabSwitchCount)
            .HasDefaultValue(0);

        builder.Property(au => au.CapturedAICount)
            .HasDefaultValue(0);
        builder.Property(au => au.AIDetectionDetails)
            .HasColumnType("TEXT");
        
        builder.Property(au => au.IsActive)
            .HasDefaultValue(true);

        // Relationships
        builder.HasOne(au => au.Assignment)
            .WithMany(a => a.AssignmentUsers)
            .HasForeignKey(au => au.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(au => au.AssignmentId);
        builder.HasIndex(au => au.UserId);
        builder.HasIndex(au => new { au.AssignmentId, au.UserId })
            .IsUnique();
        
        // Index for soft delete queries (IsActive column)
        builder.HasIndex(au => au.IsActive)
            .HasDatabaseName("idx_assignment_users_isactive");

        // Global Query Filter for soft delete
        builder.HasQueryFilter(au => au.IsActive == true);
    }
}
