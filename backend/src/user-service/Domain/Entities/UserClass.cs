using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Domain.Entities;

[Table("user_classes")]
public class UserClass
{
    [Key, Column("student_id", Order = 0)]
    [ForeignKey("Student")]
    public Guid StudentId { get; set; }
    
    [Key, Column("class_id", Order = 1)]
    [ForeignKey("Class")]
    public Guid ClassId { get; set; }
    
    [Column("joined_at")]
    public DateTime JoinedAt { get; set; }
    
    [Column("is_active")]
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual Student Student { get; set; } = null!;
    public virtual Class Class { get; set; } = null!;
}