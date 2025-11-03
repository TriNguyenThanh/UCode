using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserService.Domain.Enums;

namespace UserService.Domain.Entities;

[Table("users")]
public abstract class User
{
    [Key]
    [Column("user_id")]
    public Guid UserId { get; set; } = Guid.NewGuid();

    [MaxLength(50)]
    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("full_name")]
    public string FullName { get; set; } = string.Empty;

    [Column("role")]
    public UserRole Role { get; set; }

    [Column("status")]
    public UserStatus Status { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    
    [Column("last_login_at")]
    public DateTime? LastLoginAt { get; set; }
}