using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserService.Domain.Enums;

namespace UserService.Domain.Entities;

[Table("teachers")]
public class Teacher : User
{
    [MaxLength(20)]
    [Column("teacher_code")]
    public string TeacherCode { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("department")]
    public string Department { get; set; } = string.Empty;

    [MaxLength(50)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(15)]
    [Column("phone")]
    public string Phone { get; set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public Teacher()
    {
        Role = UserRole.Teacher;
    }
}