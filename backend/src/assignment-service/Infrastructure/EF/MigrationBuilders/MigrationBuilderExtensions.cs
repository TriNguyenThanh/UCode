using Microsoft.EntityFrameworkCore.Migrations;

namespace AssignmentService.Infrastructure.EF.MigrationBuilders;

public static class MigrationBuilderExtensions
{
    public static void CreateBestSubmissionsView(this MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP VIEW IF EXISTS best_submissions;
        ");
        
        migrationBuilder.Sql(@"
            CREATE VIEW best_submissions AS
            WITH RankedSubmissions AS (
                SELECT 
                    submission_id,
                    user_id,
                    user_full_name,
                    user_code,
                    assignment_id,
                    problem_id,
                    dataset_id,
                    source_code,
                    source_code_ref,
                    language_id,
                    language_code,
                    compare_result,
                    status,
                    is_submit_late,
                    error_code,
                    error_message,
                    total_testcase,
                    passed_testcase,
                    score,
                    comment,
                    total_time,
                    total_memory,
                    submitted_at,
                    result_file_ref,
                    COUNT(*) OVER (PARTITION BY assignment_id, user_id, problem_id) AS total_submission,
                    ROW_NUMBER() OVER (
                        PARTITION BY assignment_id, user_id, problem_id
                        ORDER BY 
                            comment DESC,
                            score DESC,
                            total_time ASC,
                            total_memory ASC,
                            submitted_at DESC
                    ) AS row_num
                FROM submission
                WHERE status IN ('Passed', 'Failed')
            )
            SELECT 
                submission_id,
                user_id,
                user_full_name,
                user_code,
                assignment_id,
                problem_id,
                dataset_id,
                source_code,
                source_code_ref,
                language_id,
                language_code,
                compare_result,
                status,
                is_submit_late,
                error_code,
                error_message,
                total_testcase,
                passed_testcase,
                score,
                comment,
                total_time,
                total_memory,
                submitted_at,
                result_file_ref,
                total_submission
            FROM RankedSubmissions
            WHERE row_num = 1;
        ");
    }

    public static void DropBestSubmissionsView(this MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP VIEW IF EXISTS best_submissions;");
    }
}
