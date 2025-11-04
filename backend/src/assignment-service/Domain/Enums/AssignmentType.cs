namespace AssignmentService.Domain.Enums;

/// <summary>
/// Loại assignment
/// </summary>
public enum AssignmentType
{
    /// <summary>
    /// Giao problem đơn lẻ
    /// </summary>
    PRACTICE,

    /// <summary>
    /// Giao examination (set problems)
    /// </summary>
    EXAMINATION,
    
    /// <summary>
    /// Giao BTVN
    /// </summary>
    HOMEWORK
}