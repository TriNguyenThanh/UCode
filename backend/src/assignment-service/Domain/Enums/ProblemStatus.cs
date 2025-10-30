namespace ProblemService.Domain.Enums;

/// <summary>
/// Trạng thái của đề bài
/// DRAFT: Đang soạn thảo
/// PUBLISHED: Đã xuất bản
/// ARCHIVED: Đã lưu trữ (không dùng nữa)
/// </summary>
public enum ProblemStatus
{
    DRAFT,
    PUBLISHED,
    ARCHIVED
}