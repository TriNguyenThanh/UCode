using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("students");

        builder.Property(s => s.StudentCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.Major)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.EnrollmentYear)
            .IsRequired();

        builder.Property(s => s.ClassYear)
            .IsRequired();

        // Index for StudentCode
        builder.HasIndex(s => s.StudentCode).IsUnique();

        // Relationship with UserClass
        builder.HasMany(s => s.UserClasses)
            .WithOne(uc => uc.Student)
            .HasForeignKey(uc => uc.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

