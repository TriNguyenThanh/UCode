using Microsoft.EntityFrameworkCore.Migrations;
using System.IO;
using System.Reflection;

public partial class AddBestSubmissionTrigger : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Đọc SQL từ embedded resource hoặc file
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "AssignmentService.Infrastructure.EF.SQL.trg_UpdateBestSubmission.sql";
        
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            // Fallback: đọc từ file path tương đối
            var sqlPath = Path.Combine(
                Path.GetDirectoryName(assembly.Location) ?? "", 
                "../../docs/sql/trg_UpdateBestSubmission.sql"
            );
            var sql = File.ReadAllText(sqlPath);
            migrationBuilder.Sql(sql);
        }
        else
        {
            using var reader = new StreamReader(stream);
            var sql = reader.ReadToEnd();
            migrationBuilder.Sql(sql);
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_UpdateBestSubmission;");
    }
}