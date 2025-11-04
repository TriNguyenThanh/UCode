using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Data.Configurations;

public class UserClassConfiguration : IEntityTypeConfiguration<UserClass>
{
    public void Configure(EntityTypeBuilder<UserClass> builder)
    {
        builder.ToTable("user_classes");

        // Composite primary key
        builder.HasKey(uc => new { uc.StudentId, uc.ClassId });

        builder.Property(uc => uc.JoinedAt)
            .IsRequired();

        builder.Property(uc => uc.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Relationships are configured in Student and Class configurations
    }
}

