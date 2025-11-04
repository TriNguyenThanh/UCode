using Microsoft.EntityFrameworkCore;
using AssignmentService.Domain.Entities;
using AssignmentService.Domain.Enums;

namespace AssignmentService.Infrastructure.EF;

/// <summary>
/// DbContext là class trung tâm để tương tác với database
/// Kế thừa từ DbContext của Entity Framework Core
/// </summary>
public class AssignmentDbContext : DbContext
{
    /// <summary>
    /// Constructor nhận DbContextOptions từ Dependency Injection
    /// options chứa connection string và các cấu hình khác
    /// </summary>
    public AssignmentDbContext(DbContextOptions<AssignmentDbContext> options)
        : base(options)
    {
    }

    /* ===== DbSet Properties =====
     * DbSet<T> đại diện cho 1 table trong database
     * Mỗi DbSet cho phép bạn query và save entities của type T
     */
    
    public DbSet<Problem> Problems { get; set; } = null!;
    public DbSet<Dataset> Datasets { get; set; } = null!;
    public DbSet<TestCase> TestCases { get; set; } = null!;
    public DbSet<Language> Languages { get; set; } = null!;
    // public DbSet<ProblemLanguage> ProblemLanguages1 { get; set; } = null!;
    public DbSet<ProblemLanguage> ProblemLanguages { get; set; } = null!;
    public DbSet<ProblemAsset> ProblemAssets { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<ProblemTag> ProblemTags { get; set; } = null!;
    public DbSet<Assignment> Assignments { get; set; } = null!;
    public DbSet<AssignmentProblem> AssignmentProblems { get; set; } = null!;
    public DbSet<AssignmentUser> AssignmentUsers { get; set; } = null!;
    public DbSet<BestSubmission> BestSubmissions { get; set; } = null!;
    public DbSet<Submission> Submissions { get; set; } = null!;

    /// <summary>
    /// Method này được gọi khi EF Core tạo model
    /// Dùng để configure relationships, constraints, indexes,...
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        /* ===== Apply Configurations =====
         * Tự động áp dụng tất cả IEntityTypeConfiguration
         * trong assembly này (Infrastructure)
         */
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssignmentDbContext).Assembly);
    }
    
    /// <summary>
    /// Configure conventions (snake_case naming)
    /// </summary>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        
        // Convert all table/column names to snake_case
        configurationBuilder.Properties<string>()
            .HaveMaxLength(4000); // Default max length for strings
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var entity = entry.Entity;
            var entityType = entity.GetType();

            // Tìm property CreatedAt và UpdatedAt bằng reflection
            var createdAtProperty = entityType.GetProperty("CreatedAt");
            var updatedAtProperty = entityType.GetProperty("UpdatedAt");

            if (entry.State == EntityState.Added)
            {
                // Khi thêm mới: gán cả CreatedAt và UpdatedAt
                if (createdAtProperty != null && createdAtProperty.PropertyType == typeof(DateTime))
                {
                    createdAtProperty.SetValue(entity, DateTime.UtcNow);
                }
                if (updatedAtProperty != null && updatedAtProperty.PropertyType == typeof(DateTime))
                {
                    updatedAtProperty.SetValue(entity, DateTime.UtcNow);
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                // Khi cập nhật: chỉ gán UpdatedAt
                if (updatedAtProperty != null && updatedAtProperty.PropertyType == typeof(DateTime))
                {
                    updatedAtProperty.SetValue(entity, DateTime.UtcNow);
                }

                // Đánh dấu CreatedAt không thay đổi
                if (createdAtProperty != null && createdAtProperty.PropertyType == typeof(DateTime))
                {
                    // Kiểm tra xem CreatedAt có giá trị không
                    var createdAtValue = createdAtProperty.GetValue(entity);
                    if (createdAtValue == null || (DateTime)createdAtValue == default)
                    {
                        // Nếu CreatedAt bị null, lấy giá trị từ database
                        var originalValue = entry.Property("CreatedAt").OriginalValue;
                        createdAtProperty.SetValue(entity, originalValue);
                    }
                    
                    // Đánh dấu không cập nhật CreatedAt
                    entry.Property("CreatedAt").IsModified = false;
                }
            }
        }
    }
}