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

        // Relationships
        builder.HasOne(au => au.Assignment)
            .WithMany(a => a.AssignmentUsers)
            .HasForeignKey(au => au.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(au => au.AssignmentId);
        builder.HasIndex(au => au.UserId);
    }
}
