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
