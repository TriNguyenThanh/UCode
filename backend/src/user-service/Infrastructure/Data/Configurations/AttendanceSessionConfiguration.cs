using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Data.Configurations;

public class AttendanceSessionConfiguration : IEntityTypeConfiguration<AttendanceSession>
{
    public void Configure(EntityTypeBuilder<AttendanceSession> builder)
    {
        builder.ToTable("attendance_sessions");

        builder.HasKey(attendanceSession => attendanceSession.Id);

        builder.Property(attendanceSession => attendanceSession.ClassId)
            .HasColumnName("class_id")
            .IsRequired();

        builder.Property(attendanceSession => attendanceSession.Title)
            .HasColumnName("title")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(attendanceSession => attendanceSession.SessionCode)
            .HasColumnName("session_code")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(attendanceSession => attendanceSession.StartTime)
            .HasColumnName("start_time")
            .IsRequired();

        builder.Property(attendanceSession => attendanceSession.EndTime)
            .HasColumnName("end_time")
            .IsRequired();

        builder.Property(attendanceSession => attendanceSession.RequireIpCheck)
            .HasColumnName("require_ip_check")
            .IsRequired();

        builder.Property(attendanceSession => attendanceSession.AllowedIpSubnet)
            .HasColumnName("allowed_ip_subnet")
            .HasMaxLength(45);

        builder.Property(attendanceSession => attendanceSession.RequireGpsCheck)
            .HasColumnName("require_gps_check")
            .IsRequired();

        builder.Property(attendanceSession => attendanceSession.AllowedLatitude)
            .HasColumnName("allowed_latitude")
            .HasColumnType("decimal(9,6)");

        builder.Property(attendanceSession => attendanceSession.AllowedLongitude)
            .HasColumnName("allowed_longitude")
            .HasColumnType("decimal(9,6)");

        builder.Property(attendanceSession => attendanceSession.AllowedRadiusMeters)
            .HasColumnName("allowed_radius_meters");

        builder.Property(attendanceSession => attendanceSession.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(attendanceSession => attendanceSession.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}