namespace UserService.Application.Events;

/// <summary>
/// Event được publish khi students được thêm vào class
/// </summary>
public class StudentsAddedToClassEvent
{
    public Guid ClassId { get; set; }
    public List<Guid> StudentIds { get; set; } = new();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Event được publish khi user bị xóa
/// </summary>
public class UserDeletedEvent
{
    public Guid UserId { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
