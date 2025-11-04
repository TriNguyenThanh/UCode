using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Domain.Entities;

[Table("classes")]
public class Class
{
    [Key]
    [Column("class_id")]
    public Guid ClassId { get; set; } = Guid.NewGuid();
    
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    [Column("description")]
    public string Description { get; set; } = string.Empty;
    
    [ForeignKey("Teacher")]
    [Column("teacher_id")]
    public Guid TeacherId { get; set; }
    
    [MaxLength(10)]
    [Column("class_code")]
    public string ClassCode { get; set; } = string.Empty;
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [Column("is_active")]
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual Teacher Teacher { get; set; } = null!;
    public virtual ICollection<UserClass> UserClasses { get; set; } = new List<UserClass>();
}