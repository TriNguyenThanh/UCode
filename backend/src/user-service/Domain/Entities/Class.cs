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
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
    
    [Column("is_active")]
    public bool IsActive { get; set; } = true;
    
    [Column("is_archived")]
    public bool IsArchived { get; set; } = false;
    
    [Column("archived_at")]
    public DateTime? ArchivedAt { get; set; }
    
    [MaxLength(500)]
    [Column("archive_reason")]
    public string? ArchiveReason { get; set; }
    
    // Navigation properties
    public virtual Teacher Teacher { get; set; } = null!;
    public virtual ICollection<UserClass> UserClasses { get; set; } = new List<UserClass>();
}