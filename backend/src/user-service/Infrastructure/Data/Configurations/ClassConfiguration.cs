using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Data.Configurations;

public class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.ToTable("classes");

        builder.HasKey(c => c.ClassId);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.TeacherId)
            .IsRequired();

        builder.Property(c => c.ClassCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Index for ClassCode
        builder.HasIndex(c => c.ClassCode).IsUnique();

        // Relationship with UserClass
        builder.HasMany(c => c.UserClasses)
            .WithOne(uc => uc.Class)
            .HasForeignKey(uc => uc.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with Teacher is configured in TeacherConfiguration
    }
}

