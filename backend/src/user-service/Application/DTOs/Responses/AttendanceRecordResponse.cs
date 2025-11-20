namespace UserService.Application.DTOs.Responses;

using System;

public class AttendanceRecordResponse
{
    /// <summary>
    /// ID bản ghi điểm danh (khóa chính)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID phiên điểm danh (link tới AttendanceSession)
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    /// ID sinh viên điểm danh
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Thời gian thực hiện điểm danh
    /// </summary>
    public DateTime AttendedAt { get; set; }

    /// <summary>
    /// IP thực tế của sinh viên
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Vĩ độ thực tế của sinh viên
    /// </summary>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// Kinh độ thực tế của sinh viên
    /// </summary>
    public decimal? Longitude { get; set; }

    /// <summary>
    /// Thiết bị sử dụng (User-Agent)
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Định danh thiết bị
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// Kết quả so sánh với config (true/false)
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Lý do fail (VD: Wrong IP, Too far)
    /// </summary>
    public string? InvalidReason { get; set; }
}
