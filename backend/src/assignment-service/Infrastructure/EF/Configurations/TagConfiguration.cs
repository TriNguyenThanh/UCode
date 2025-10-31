using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssignmentService.EF.Entities;

namespace AssignmentService.Infrastructure.EF.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey(t => t.TagId);
        
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(64);
        
        builder.Property(t => t.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(16);
        
        builder.HasIndex(t => t.Name)
            .IsUnique();
        
        builder.HasIndex(t => t.Category);
    }
}