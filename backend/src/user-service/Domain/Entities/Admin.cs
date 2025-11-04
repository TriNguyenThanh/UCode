using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserService.Domain.Enums;

namespace UserService.Domain.Entities;

[Table("admins")]
public class Admin : User
{
    public Admin()
    {
        Role = UserRole.Admin;
    }
}