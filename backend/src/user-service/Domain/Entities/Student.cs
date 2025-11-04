using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserService.Domain.Enums;

namespace UserService.Domain.Entities;

[Table("students")]
public class Student : User
{
    [MaxLength(20)]
    [Column("student_code")]
    public string StudentCode { get; set; } = string.Empty;

    [Column("enrollment_year")]
    public int EnrollmentYear { get; set; }

    [MaxLength(100)]
    [Column("major")]
    public string Major { get; set; } = string.Empty;

    [Column("class_year")]
    public int ClassYear { get; set; }

    public virtual ICollection<UserClass> UserClasses { get; set; } = new List<UserClass>();

    public Student()
    {
        Role = UserRole.Student;
    }
}