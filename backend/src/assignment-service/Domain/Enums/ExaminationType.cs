namespace AssignmentService.Domain.Enums;

/// <summary>
/// Loại đợt kiểm tra
/// </summary>
public enum ExaminationType
{
    /// <summary>
    /// Chế độ luyện tập - không giới hạn thời gian
    /// </summary>
    PRACTICE = 0,

    /// <summary>
    /// Chế độ thi - có giới hạn thời gian
    /// </summary>
    CONTEST = 1
}