using AssignmentService.Domain.Entities;

namespace AssignmentService.Application.Interfaces.Repositories;

public interface IAssignmentRepository : IRepository<Assignment>
{
    Task<Assignment?> GetByIdWithDetailsAsync(Guid id);
    Task<Assignment?> GetByIdWithProblemBasicsAsync(Guid id); // Chỉ lấy ID, Title, Code, Difficulty của problems
    Task<List<Assignment>> GetByTeacherIdAsync(Guid teacherId);
    Task<List<Assignment>> GetByClassIdAsync(Guid classId);
    Task<List<Assignment>> GetByUserIdAsync(Guid studentId);
    Task<Assignment?> GetWithStatisticsAsync(Guid id);
    
    // Lightweight queries for ownership verification
    Task<Guid?> GetAssignmentOwnerIdAsync(Guid assignmentId);
    Task<(bool exists, Guid? ownerId)> CheckAssignmentExistsAndGetOwnerAsync(Guid assignmentId);
    
    // AssignmentUser operations
    Task<AssignmentUser> AddAssignmentUserAsync(AssignmentUser detail);
    Task<List<AssignmentUser>> AddAssignmentUsersAsync(List<AssignmentUser> details);
    Task<AssignmentUser?> GetAssignmentUserAsync(Guid assignmentId, Guid studentId);
    Task<AssignmentUser?> GetAssignmentUserByIdAsync(Guid AssignmentUserId);
    Task<List<AssignmentUser>> GetAssignmentUsersByAssignmentAsync(Guid assignmentId);
    Task<AssignmentUser> UpdateAssignmentUserAsync(AssignmentUser detail);
    Task RemoveAssignmentUsersByAssignmentAsync(Guid assignmentId);

    // MaxScore helpers
    Task<int> GetAssignmentMaxScoreAsync(Guid assignmentId);
    Task UpdateAssignmentUsersMaxScoreAsync(Guid assignmentId, int maxScore);
    
    // AssignmentProblem operations
    Task<List<AssignmentProblem>> AddAssignmentProblemsAsync(List<AssignmentProblem> assignmentProblems);
    Task<AssignmentProblem?> GetAssignmentProblemAsync(Guid assignmentId, Guid problemId);
    
    // BestSubmission operations
    // Task<BestSubmission> AddSubmissionAsync(BestSubmission submission);
    // Task<List<BestSubmission>> AddSubmissionsAsync(List<BestSubmission> submissions);
    // Task<BestSubmission?> GetSubmissionAsync(Guid AssignmentUserId, Guid problemId);
    // Task<BestSubmission?> GetSubmissionByIdAsync(Guid submissionId);
    // Task<List<BestSubmission>> GetSubmissionsByAssignmentUserAsync(Guid AssignmentUserId);
    // Task<List<BestSubmission>> GetSubmissionsByAssignmentAsync(Guid assignmentId);
    // Task<BestSubmission> UpdateSubmissionAsync(BestSubmission submission);
    // Task<bool> DeleteSubmissionAsync(Guid submissionId);
    
    // ExamActivityLog operations
    Task<ExamActivityLog> AddExamActivityLogAsync(ExamActivityLog activity);
    Task<List<ExamActivityLog>> AddExamActivityLogsBatchAsync(List<ExamActivityLog> activities);
    Task<List<ExamActivityLog>> GetExamActivityLogsByAssignmentUserAsync(Guid assignmentUserId);
}