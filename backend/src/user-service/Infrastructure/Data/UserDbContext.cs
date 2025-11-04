using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Infrastructure.Data.Configurations;

namespace UserService.Infrastructure.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Class> Classes { get; set; }
    public DbSet<UserClass> UserClasses { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure inheritance using TPT (Table-per-Type)
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<Student>().ToTable("students");
        modelBuilder.Entity<Teacher>().ToTable("teachers");
        modelBuilder.Entity<Admin>().ToTable("admins");

        // Apply all entity configurations from Configurations folder
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new StudentConfiguration());
        modelBuilder.ApplyConfiguration(new TeacherConfiguration());
        modelBuilder.ApplyConfiguration(new AdminConfiguration());
        modelBuilder.ApplyConfiguration(new ClassConfiguration());
        modelBuilder.ApplyConfiguration(new UserClassConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
    }
}