namespace AssignmentService.Domain.Enums;

/// <summary>
/// Quyền truy cập đề bài
/// PRIVATE: Chỉ owner
/// COURSE: Trong khóa học
/// PUBLIC: Công khai
/// </summary>
public enum Visibility
{
    PRIVATE,
    COURSE,
    PUBLIC
}