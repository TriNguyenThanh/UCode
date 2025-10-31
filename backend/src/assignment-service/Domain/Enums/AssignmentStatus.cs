namespace AssignmentService.Domain.Enums;

public enum AssignmentStatus
{
    DRAFT = 0,      // Nháp, chưa giao
    PUBLISHED = 1,  // Đã giao cho students
    CLOSED = 2      // Đã đóng, không nhận bài nữa
}
