namespace AssignmentService.Domain.Enums;

/// <summary>
/// Trạng thái đợt kiểm tra
/// </summary>
public enum ExaminationStatus
{
    /// <summary>
    /// Đang hoạt động - users có thể tham gia
    /// </summary>
    ACTIVE = 0,

    /// <summary>
    /// Tạm dừng - users không thể tham gia
    /// </summary>
    INACTIVE = 1,

    /// <summary>
    /// Đã lưu trữ - đợt kiểm tra đã kết thúc
    /// </summary>
    ARCHIVED = 2
}