using Microsoft.EntityFrameworkCore.Migrations;

namespace AssignmentService.Infrastructure.EF.MigrationBuilders;

public static class MigrationBuilderExtensions
{
    public static void CreateBestSubmissionsView(this MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE VIEW BestSubmissions AS
            WITH RankedSubmissions AS (
                SELECT 
                    submission_id,
                    assignment_id,
                    user_id,
                    problem_id,
                    score,
                    total_time,
                    total_memory,
                    submitted_at,   
                    ROW_NUMBER() OVER (
                        PARTITION BY assignment_id, user_id, problem_id
                        ORDER BY 
                            -- Điểm cao nhất
                            score DESC,
                            -- Thời gian nhanh nhất
                            total_time ASC,
                            -- Bộ nhớ ít nhất
                            total_memory ASC,
                            -- Submission mới nhất
                            submitted_at DESC
                    ) AS RowNum
                FROM submission
                WHERE status IN ('Passed', 'Failed') -- Only completed submissions
            )
            SELECT 
                NEWID() AS BestSubmissionId,
                assignment_id AS AssignmentId,
                user_id AS UserId,
                problem_id AS ProblemId,
                submission_id AS SubmissionId,
                score AS Score,
                total_time AS TotalTime,
                total_memory AS TotalMemory,
                submitted_at AS SubmitAt
            FROM RankedSubmissions
            WHERE RowNum = 1;
        ");
    }

    public static void DropBestSubmissionsView(this MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP VIEW IF EXISTS BestSubmissions;");
    }
}
