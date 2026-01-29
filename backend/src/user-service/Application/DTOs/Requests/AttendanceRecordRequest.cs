namespace UserService.Application.DTOs.Requests;

using System;

public class AttendanceRecordRequest
{
    /// <summary>
    /// ID phiên điểm danh (link tới AttendanceSession)
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    /// Mã  điểm danh
    /// </summary>
    public string SessionCode { get; set; } = string.Empty;
    
    /// <summary>
    /// ID sinh viên điểm danh
    /// </summary>
    public Guid UserId { get; set; }
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
    /// Ảnh khuôn mặt dạng base64 (dùng cho xác thực khuôn mặt)
    /// </summary>
    public string? FaceImage { get; set; }
}
