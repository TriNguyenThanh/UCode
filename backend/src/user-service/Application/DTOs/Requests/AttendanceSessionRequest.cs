namespace UserService.Application.DTOs.Requests;

using System;

public class AttendanceSessionRequest
{
    /// <summary>
    /// ID lớp được điểm danh (khóa ngoại)
    /// </summary>
    public Guid ClassId { get; set; }

    /// <summary>
    /// Tiêu đề, ví dụ: "Điểm danh tuần 1"
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Mã unique để tạo URL điểm danh
    /// </summary>
    public string SessionCode { get; set; } = string.Empty;

    /// <summary>
    /// Thời gian bắt đầu mở link
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Thời gian link hết hạn
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Cấu hình: Bắt buộc check IP hay không
    /// </summary>
    public bool RequireIpCheck { get; set; }

    /// <summary>
    /// IP/Subnet cho phép (VD: "192.168.1.1")
    /// </summary>
    public string? AllowedIpSubnet { get; set; }

    /// <summary>
    /// Cấu hình: Bắt buộc check GPS hay không
    /// </summary>
    public bool RequireGpsCheck { get; set; }

    /// <summary>
    /// Vĩ độ cho phép
    /// </summary>
    public decimal? AllowedLatitude { get; set; }

    /// <summary>
    /// Kinh độ cho phép
    /// </summary>
    public decimal? AllowedLongitude { get; set; }

    /// <summary>
    /// Bán kính cho phép (mét)
    /// </summary>
    public int? AllowedRadiusMeters { get; set; }

    /// <summary>
    /// Trạng thái kích hoạt
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// ID của phiên điểm danh (nếu có)
    /// </summary>
    public Guid? Id { get; set; }
}
