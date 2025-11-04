# Hướng dẫn tạo Migration có VIEW khi Repository không có Migrations

> **Tình huống:** Bạn clone repository nhưng thư mục `Migrations/` không được commit. Bạn cần tạo lại migrations bao gồm cả VIEW.

## 📋 Prerequisites

- .NET 8.0 SDK
- SQL Server
- EF Core CLI tools
- File `MigrationBuilderExtensions.cs` đã tồn tại trong `Infrastructure/EF/MigrationBuilders/`

---

## 🚀 Các bước thực hiện

### Bước 1: Kiểm tra cấu trúc hiện tại

```bash
cd backend/src/assignment-service

# Kiểm tra không có thư mục Migrations
ls -la Infrastructure/Migrations/  # Nên không tồn tại hoặc rỗng

# Kiểm tra MigrationBuilderExtensions có tồn tại
ls -la Infrastructure/EF/MigrationBuilders/MigrationBuilderExtensions.cs
```

### Bước 2: Tạo Migration đầu tiên (Tables)

```bash
# Xóa migration cũ nếu có
rm -rf Infrastructure/Migrations/

# Tạo migration mới cho tất cả entities
dotnet ef migrations add InitialCreate --project Infrastructure --startup-project Api

# Output:
# Build succeeded.
# Done. To undo this action, use 'ef migrations remove'
```

**Kết quả:** EF Core sẽ tạo migration với tất cả tables dựa trên entities.

### Bước 3: Tạo Migration cho VIEW

```bash
# Tạo migration riêng cho VIEW
dotnet ef migrations add CreateBestSubmissionsView --project Infrastructure --startup-project Api
```

**Kết quả:** File migration **RỖNG** được tạo tại:
```
Infrastructure/Migrations/YYYYMMDDHHMMSS_CreateBestSubmissionsView.cs
```

> ⚠️ **LƯU Ý QUAN TRỌNG:**  
> Dù đã có sẵn file `MigrationBuilderExtensions.cs` với logic `CreateBestSubmissionsView()`, EF Core **KHÔNG TỰ ĐỘNG** thêm hàm này vào migration. Bạn **PHẢI THỦ CÔNG** thêm vào Bước 4 bên dưới.

### Bước 4: Thêm logic tạo VIEW vào Migration

Mở file `Infrastructure/Migrations/YYYYMMDDHHMMSS_CreateBestSubmissionsView.cs` và sửa:

```csharp
using Microsoft.EntityFrameworkCore.Migrations;
using AssignmentService.Infrastructure.EF.MigrationBuilders;  // ✅ Thêm using này

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateBestSubmissionsView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ Gọi extension method để tạo VIEW
            migrationBuilder.CreateBestSubmissionsView();
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ✅ Gọi extension method để xóa VIEW
            migrationBuilder.DropBestSubmissionsView();
        }
    }
}
```

### Bước 5: Apply Migrations

```bash
# Tạo database và apply tất cả migrations
dotnet ef database update --project Infrastructure --startup-project Api

# Output:
# Build succeeded.
# Applying migration '20251102194201_InitialCreate'.
# Applying migration '20251102194458_CreateBestSubmissionsView'.
# Done.
```

### Bước 6: Verify VIEW đã được tạo

```bash
# Kiểm tra VIEW trong SQL Server
dotnet ef dbcontext script --project Infrastructure --startup-project Api | grep -A 20 "CREATE VIEW"
```

Hoặc connect vào SQL Server và chạy:
```sql
SELECT * FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME = 'BestSubmissions';
```

---

## 📁 Cấu trúc file sau khi hoàn thành

```
Infrastructure/
├── EF/
│   ├── Configurations/
│   │   └── BestSubmissionConfiguration.cs  ✅ ToView("BestSubmissions") + HasNoKey()
│   └── MigrationBuilders/
│       ├── MigrationBuilderExtensions.cs   ✅ CreateBestSubmissionsView() + DropBestSubmissionsView()
│       └── CreateBestSubmissionsView.cs    ✅ Template SQL (optional)
└── Migrations/
    ├── 20251102194201_InitialCreate.cs                  ✅ Tạo tables
    ├── 20251102194201_InitialCreate.Designer.cs
    ├── 20251102194458_CreateBestSubmissionsView.cs      ✅ Tạo VIEW
    ├── 20251102194458_CreateBestSubmissionsView.Designer.cs
    └── AssignmentDbContextModelSnapshot.cs
```

---

## 🔧 Troubleshooting

### Lỗi: "Incorrect syntax near the keyword 'ON'"

**Nguyên nhân:** SQL Server không hỗ trợ `CROSS JOIN ... ON`

**Giải pháp:** Sửa trong `MigrationBuilderExtensions.cs`:

```csharp
// ❌ SAI
FROM assignment_user au
CROSS JOIN assignment_problem ap 
    ON ap.assignment_id = au.assignment_id

// ✅ ĐÚNG
FROM assignment_user au
INNER JOIN assignment_problem ap 
    ON ap.assignment_id = au.assignment_id
```

### Lỗi: "There is already an object named 'xxx' in the database"

**Nguyên nhân:** Database đã có tables từ trước

**Giải pháp:**
```bash
# Option 1: Drop và tạo lại database
dotnet ef database drop --project Infrastructure --startup-project Api --force
dotnet ef database update --project Infrastructure --startup-project Api

# Option 2: Hoặc tạo migration từ database hiện tại
dotnet ef migrations add InitialCreate --project Infrastructure --startup-project Api --context-namespace "Infrastructure.Migrations"
```

### Migration mới không chứa VIEW

**Nguyên nhân:** EF Core chỉ tạo migration cho entities, không tự động detect VIEWs

**Giải pháp:** Phải **thủ công** tạo migration và gọi `migrationBuilder.CreateBestSubmissionsView()`

---

## 📝 Template MigrationBuilderExtensions.cs

Nếu file `MigrationBuilderExtensions.cs` không tồn tại, tạo file mới:

```csharp
using Microsoft.EntityFrameworkCore.Migrations;

namespace AssignmentService.Infrastructure.EF.MigrationBuilders;

public static class MigrationBuilderExtensions
{
    public static void CreateBestSubmissionsView(this MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE VIEW BestSubmissions AS
            SELECT 
                NEWID() AS BestSubmissionId,
                au.assignment_user_id AS AssignmentUserId,
                ap.problem_id AS ProblemId,
                s.SubmissionId,
                s.Score,
                ap.points AS MaxScore,
                s.TotalTime,
                s.TotalMemory,
                s.SubmittedAt AS UpdatedAt
            FROM assignment_user au
            INNER JOIN assignment_problem ap 
                ON ap.assignment_id = au.assignment_id
            CROSS APPLY (
                SELECT TOP 1
                    sub.submission_id AS SubmissionId,
                    CASE 
                        WHEN sub.total_testcase = 0 THEN 0
                        ELSE (sub.passed_testcase * 100) / sub.total_testcase
                    END AS Score,
                    sub.total_time AS TotalTime,
                    sub.total_memory AS TotalMemory,
                    sub.submitted_at AS SubmittedAt
                FROM submission sub
                WHERE sub.assignment_user_id = au.assignment_user_id
                    AND sub.problem_id = ap.problem_id
                    AND sub.status = 4
                ORDER BY 
                    CASE 
                        WHEN sub.total_testcase = 0 THEN 0
                        ELSE (sub.passed_testcase * 100) / sub.total_testcase
                    END DESC,
                    sub.total_time ASC,
                    sub.total_memory ASC,
                    sub.submitted_at ASC
            ) s;
        ");
    }

    public static void DropBestSubmissionsView(this MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP VIEW IF EXISTS BestSubmissions;");
    }
}
```

---

## 🎯 Best Practices

### ✅ DO

1. **Commit thư mục `Migrations/` vào Git**
   ```bash
   git add Infrastructure/Migrations/
   git add Infrastructure/EF/MigrationBuilders/
   git commit -m "Add migrations and VIEW creation"
   ```

2. **Tạo migration riêng cho VIEW**
   - Không trộn lẫn tạo tables và VIEW trong cùng 1 migration
   - Dễ rollback và debug

3. **Sử dụng Extension Methods**
   - Tái sử dụng SQL logic
   - Dễ maintain

4. **Tự động migrate khi start app** (trong `Program.cs`)
   ```csharp
   await context.Database.MigrateAsync();
   ```

### ❌ DON'T

1. **Không commit `Migrations/`**
   - Team members không có schema history
   - Khó đồng bộ database giữa các môi trường

2. **Hardcode SQL trong migration**
   - Khó tái sử dụng
   - Khó maintain khi cần update VIEW

3. **Tạo VIEW thủ công trong database**
   - Mất tính tự động hóa
   - Không có version control

---

## 🚢 Deploy to Production

### Option 1: Auto-migrate (Development/Staging)

Code đã có sẵn trong `Program.cs`:
```csharp
await context.Database.MigrateAsync();
```

### Option 2: Generate SQL Script (Production)

```bash
# Generate SQL script từ tất cả migrations
dotnet ef migrations script --project Infrastructure --startup-project Api --output schema.sql

# Review script trước khi chạy
cat schema.sql

# Chạy trên production database
sqlcmd -S production-server -d AssignmentServiceDb -i schema.sql
```

### Option 3: Manual Migration Command (Production)

```bash
# Trên production server
dotnet ef database update --project Infrastructure --startup-project Api --connection "Server=prod-server;Database=..."
```

---

## 📚 Tài liệu tham khảo

- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [SQL Server Views](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-view-transact-sql)
- [EF Core Raw SQL](https://learn.microsoft.com/en-us/ef/core/querying/raw-sql)

---

## ✅ Checklist

Sau khi hoàn thành, kiểm tra:

- [ ] File `MigrationBuilderExtensions.cs` tồn tại
- [ ] Migration `InitialCreate` đã được tạo
- [ ] Migration `CreateBestSubmissionsView` đã được tạo và có gọi extension method
- [ ] `BestSubmissionConfiguration.cs` có `.ToView("BestSubmissions")` và `.HasNoKey()`
- [ ] Database đã được tạo và migrations đã apply
- [ ] VIEW `BestSubmissions` tồn tại trong database
- [ ] Code có thể query VIEW: `await _context.BestSubmissions.ToListAsync()`
- [ ] Thư mục `Migrations/` và `MigrationBuilders/` đã commit vào Git

---

**Author:** Assignment Service Team  
**Last Updated:** November 3, 2025  
**Version:** 1.0
