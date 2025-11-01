using AssignmentService.Domain.Entities;
using AssignmentService.Application.DTOs.Responses;

namespace AssignmentService.Application.Interfaces.Services;

public interface IAssignmentService
{
    // Assignment CRUD
    Task<Assignment> CreateAssignmentAsync(Assignment assignment);
    Task<Assignment> UpdateAssignmentAsync(Assignment assignment);
    Task<bool> DeleteAssignmentAsync(Guid assignmentId);
    Task<Assignment?> GetAssignmentByIdAsync(Guid assignmentId);
    Task<List<Assignment>> GetAssignmentsByTeacherAsync(Guid teacherId);
    Task<List<Assignment>> GetAssignmentsByStudentAsync(Guid studentId);
    Task<List<Assignment>> GetAssignmentsByClassIdAsync(Guid classId);
    
    // Lightweight queries for ownership verification
    Task<Guid?> GetAssignmentOwnerIdAsync(Guid assignmentId);
    Task<(bool exists, Guid? ownerId)> CheckAssignmentExistsAndGetOwnerAsync(Guid assignmentId);
    
    // Assignment Details
    Task<AssignmentUser?> GetAssignmentUserByIdAsync(Guid assignmentDetailId);
    Task<AssignmentUser?> GetAssignmentUserAsync(Guid assignmentId, Guid studentId);
    Task<List<AssignmentUser>> GetAssignmentUsersAsync(Guid assignmentId);
    Task<AssignmentUser> UpdateAssignmentUserAsync(AssignmentUser detail);
    
    // BestSubmission operations
    Task<BestSubmission> SaveSubmissionAsync(BestSubmission submission);
    Task<BestSubmission> UpdateSubmissionAsync(BestSubmission submission);
    Task<bool> DeleteSubmissionAsync(Guid submissionId);
    Task<BestSubmission?> GetSubmissionAsync(Guid assignmentDetailId, Guid problemId);
    Task<List<BestSubmission>> GetSubmissionsByAssignmentUserAsync(Guid assignmentDetailId);
    Task<List<BestSubmission>> GetSubmissionsByAssignmentAsync(Guid assignmentId);
    Task<List<BestSubmission>> CreateSubmissionsForAssignmentUserAsync(Guid assignmentDetailId, List<Guid> problemIds);
    
    // AssignmentProblem operations
    Task<AssignmentProblem?> GetAssignmentProblemAsync(Guid assignmentId, Guid problemId);
    
    // Statistics
    Task<AssignmentStatistics> GetAssignmentStatisticsAsync(Guid assignmentId);
}
