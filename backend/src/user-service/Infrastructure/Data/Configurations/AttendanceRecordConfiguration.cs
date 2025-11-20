using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Data.Configurations;

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("attendance_records");

        builder.HasKey(ar => ar.Id);

        builder.Property(ar => ar.SessionId)
            .HasColumnName("session_id")
            .IsRequired();

        builder.Property(ar => ar.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(ar => ar.AttendedAt)
            .HasColumnName("attended_at")
            .IsRequired();

        builder.Property(ar => ar.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45); // IPv6 max length

        builder.Property(ar => ar.Latitude)
            .HasColumnName("latitude")
            .HasColumnType("decimal(9,6)");

        builder.Property(ar => ar.Longitude)
            .HasColumnName("longitude")
            .HasColumnType("decimal(9,6)");

        builder.Property(ar => ar.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(255);

        builder.Property(ar => ar.DeviceId)
            .HasColumnName("device_id")
            .HasMaxLength(255);

        builder.Property(ar => ar.InvalidReason)
            .HasColumnName("invalid_reason")
            .HasMaxLength(255);

        builder.HasIndex(ar => new { ar.SessionId, ar.UserId })
            .IsUnique();

        builder.HasOne<AttendanceSession>()
            .WithMany()
            .HasForeignKey(ar => ar.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}