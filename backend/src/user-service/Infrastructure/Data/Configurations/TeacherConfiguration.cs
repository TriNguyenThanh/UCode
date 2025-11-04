using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Data.Configurations;

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("teachers");

        builder.Property(t => t.TeacherCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(t => t.Department)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Title)
            .HasMaxLength(50);

        builder.Property(t => t.Phone)
            .HasMaxLength(15);

        // Index for TeacherCode
        builder.HasIndex(t => t.TeacherCode).IsUnique();

        // Relationship with Classes
        builder.HasMany(t => t.Classes)
            .WithOne(c => c.Teacher)
            .HasForeignKey(c => c.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

