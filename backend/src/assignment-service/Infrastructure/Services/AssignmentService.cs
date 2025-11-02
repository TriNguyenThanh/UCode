using AssignmentService.Application.Interfaces.Repositories;
using AssignmentService.Application.Interfaces.Services;
using AssignmentService.Application.DTOs.Responses;
using AssignmentService.Domain.Entities;
using AssignmentService.Domain.Enums;

namespace AssignmentService.Infrastructure.Services;

public class AssignmentService : IAssignmentService
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IUserServiceClient _userServiceClient;

    public AssignmentService(IAssignmentRepository assignmentRepository, IUserServiceClient userServiceClient)
    {
        _assignmentRepository = assignmentRepository;
        _userServiceClient = userServiceClient;
    }

    public async Task<Assignment> CreateAssignmentAsync(Assignment assignment)
    {
        assignment.AssignmentId = Guid.NewGuid();
        assignment.AssignedAt = DateTime.UtcNow;
        
        var createdAssignment = await _assignmentRepository.AddAsync(assignment);

        // Compute MaxScore from AssignmentProblems
        var maxScore = await _assignmentRepository.GetAssignmentMaxScoreAsync(createdAssignment.AssignmentId);

        // Fetch class roster from UserService and create AssignmentUsers 
        var userIds = await _userServiceClient.GetStudentIdsByClassIdAsync(assignment.ClassId);
        if (userIds != null && userIds.Count > 0)
        {
            var details = userIds.Select(userId => new AssignmentUser
            {
                AssignmentUserId = Guid.NewGuid(),
                AssignmentId = createdAssignment.AssignmentId,
                UserId = userId,
                Status = AssignmentUserStatus.NOT_STARTED,
                AssignedAt = DateTime.UtcNow,
                MaxScore = maxScore
            }).ToList();

            await _assignmentRepository.AddAssignmentUsersAsync(details);
        }

        return createdAssignment;
    }

    public async Task<Assignment> UpdateAssignmentAsync(Assignment assignment)
    {
        // Load current to compare
        var current = await _assignmentRepository.GetByIdAsync(assignment.AssignmentId)
            ?? throw new KeyNotFoundException("Assignment not found");

        var isClassChanged = current.ClassId != assignment.ClassId;

        // Only allow changing ClassId when DRAFT
        if (isClassChanged && current.Status != AssignmentStatus.DRAFT)
        {
            throw new InvalidOperationException("Cannot change ClassId unless assignment is in DRAFT status.");
        }

        var updated = await _assignmentRepository.UpdateAsync(assignment);

        // Resync AssignmentUsers if class changed and assignment is DRAFT
        if (isClassChanged && updated.Status == AssignmentStatus.DRAFT)
        {
            await _assignmentRepository.RemoveAssignmentUsersByAssignmentAsync(updated.AssignmentId);

            var userIds = await _userServiceClient.GetStudentIdsByClassIdAsync(updated.ClassId);
            if (userIds != null && userIds.Count > 0)
            {
                var maxScore = await _assignmentRepository.GetAssignmentMaxScoreAsync(updated.AssignmentId);
                var details = userIds.Select(userId => new AssignmentUser
                {
                    AssignmentUserId = Guid.NewGuid(),
                    AssignmentId = updated.AssignmentId,
                    UserId = userId,
                    Status = AssignmentUserStatus.NOT_STARTED,
                    AssignedAt = DateTime.UtcNow,
                    MaxScore = maxScore
                }).ToList();

                await _assignmentRepository.AddAssignmentUsersAsync(details);
            }
        }

        return updated;
    }

    public async Task<bool> DeleteAssignmentAsync(Guid assignmentId)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
        if (assignment == null)
            return false;
            
        await _assignmentRepository.RemoveAsync(assignment.AssignmentId);
        return true;
    }

    public async Task<Assignment?> GetAssignmentByIdAsync(Guid assignmentId)
    {
        return await _assignmentRepository.GetByIdWithDetailsAsync(assignmentId);
    }

    public async Task<Assignment?> GetAssignmentWithProblemBasicsAsync(Guid assignmentId)
    {
        return await _assignmentRepository.GetByIdWithProblemBasicsAsync(assignmentId);
    }

    
    public async Task<List<Assignment>> GetAssignmentsByTeacherAsync(Guid teacherId)
    {
        return await _assignmentRepository.GetByTeacherIdAsync(teacherId);
    }

    public async Task<List<Assignment>> GetAssignmentsByStudentAsync(Guid userId)
    {
        return await _assignmentRepository.GetByUserIdAsync(userId);
    }

    public async Task<List<Assignment>> GetAssignmentsByClassIdAsync(Guid classId)
    {
        return await _assignmentRepository.GetByClassIdAsync(classId);
    }

    public async Task<AssignmentUser?> GetAssignmentUserAsync(Guid assignmentId, Guid userId)
    {
        return await _assignmentRepository.GetAssignmentUserAsync(assignmentId, userId);
    }

    public async Task<AssignmentUser?> GetAssignmentUserByIdAsync(Guid assignmentUserId)
    {
        return await _assignmentRepository.GetAssignmentUserByIdAsync(assignmentUserId);
    }

    public async Task<List<AssignmentUser>> GetAssignmentUsersAsync(Guid assignmentId)
    {
        return await _assignmentRepository.GetAssignmentUsersByAssignmentAsync(assignmentId);
    }

    public async Task<AssignmentUser> UpdateAssignmentUserAsync(AssignmentUser detail)
    {
        return await _assignmentRepository.UpdateAssignmentUserAsync(detail);
    }

    public async Task<AssignmentStatistics> GetAssignmentStatisticsAsync(Guid assignmentId)
    {
        var assignment = await _assignmentRepository.GetWithStatisticsAsync(assignmentId);
        if (assignment == null)
            throw new KeyNotFoundException("Assignment not found");
        
        var details = assignment.AssignmentUsers.ToList();
        var total = details.Count;
        
        if (total == 0)
        {
            return new AssignmentStatistics
            {
                TotalStudents = 0,
                CompletionRate = 0,
                AverageScore = 0
            };
        }
        
        var graded = details.Where(d => d.Score.HasValue).ToList();
        
        return new AssignmentStatistics
        {
            TotalStudents = total,
            NotStarted = details.Count(d => d.Status == AssignmentUserStatus.NOT_STARTED),
            InProgress = details.Count(d => d.Status == AssignmentUserStatus.IN_PROGRESS),
            Submitted = details.Count(d => d.Status == AssignmentUserStatus.SUBMITTED),
            Graded = details.Count(d => d.Status == AssignmentUserStatus.GRADED),
            AverageScore = graded.Any() ? graded.Average(d => d.Score!.Value) : 0,
            CompletionRate = (double)details.Count(d => d.Status >= AssignmentUserStatus.SUBMITTED) / total * 100
        };
    }

    // BestSubmission methods
    public async Task<BestSubmission> SaveSubmissionAsync(BestSubmission submission)
    {
        // submission.SubmissionId = Guid.NewGuid();
        // submission.Status = Domain.Enums.BestSubmissionStatus.NOT_STARTED;
        // submission.AttemptCount = 0;
        var existSubmission = await _assignmentRepository.GetSubmissionByIdAsync(submission.SubmissionId);
        if (existSubmission != null)
            return await _assignmentRepository.UpdateSubmissionAsync(submission);
        else
            return await _assignmentRepository.AddSubmissionAsync(submission);
    }

    public async Task<BestSubmission> UpdateSubmissionAsync(BestSubmission submission)
    {
        return await _assignmentRepository.UpdateSubmissionAsync(submission);
    }

    public async Task<bool> DeleteSubmissionAsync(Guid submissionId)
    {
        return await _assignmentRepository.DeleteSubmissionAsync(submissionId);
    }

    public async Task<BestSubmission?> GetSubmissionAsync(Guid assignmentUserId, Guid problemId)
    {
        return await _assignmentRepository.GetSubmissionAsync(assignmentUserId, problemId);
    }

    public async Task<List<BestSubmission>> GetSubmissionsByAssignmentUserAsync(Guid assignmentUserId)
    {
        return await _assignmentRepository.GetSubmissionsByAssignmentUserAsync(assignmentUserId);
    }

    public async Task<List<BestSubmission>> GetSubmissionsByAssignmentAsync(Guid assignmentId)
    {
        return await _assignmentRepository.GetSubmissionsByAssignmentAsync(assignmentId);
    }

    public async Task<List<BestSubmission>> CreateSubmissionsForAssignmentUserAsync(Guid assignmentUserId, List<Guid> problemIds)
    {

        throw new NotSupportedException("Cái method này để dành cho Trí làm nhé");
        // var submissions = new List<BestSubmission>();
        
        // // Lấy AssignmentUser để có AssignmentId
        // var assignmentUser = await _assignmentRepository.GetAssignmentUserByIdAsync(assignmentUserId);
        // if (assignmentUser == null)
        //     throw new KeyNotFoundException("AssignmentUser not found");
        
        // foreach (var problemId in problemIds)
        // {
        //     var submission = new BestSubmission
        //     {
        //         SubmissionId = Guid.NewGuid(),
        //         AssignmentUserId = assignmentUserId,
        //         ProblemId = problemId,
        //         Status = Domain.Enums.BestSubmissionStatus.NOT_STARTED,
        //         AttemptCount = 0
        //     };
            
        //     submissions.Add(submission);
        // }
        
        // return await _assignmentRepository.AddSubmissionsAsync(submissions);
    }

    public async Task<AssignmentProblem?> GetAssignmentProblemAsync(Guid assignmentId, Guid problemId)
    {
        return await _assignmentRepository.GetAssignmentProblemAsync(assignmentId, problemId);
    }

    public async Task<Guid?> GetAssignmentOwnerIdAsync(Guid assignmentId)
    {
        return await _assignmentRepository.GetAssignmentOwnerIdAsync(assignmentId);
    }

    public async Task<(bool exists, Guid? ownerId)> CheckAssignmentExistsAndGetOwnerAsync(Guid assignmentId)
    {
        return await _assignmentRepository.CheckAssignmentExistsAndGetOwnerAsync(assignmentId);
    }
}
