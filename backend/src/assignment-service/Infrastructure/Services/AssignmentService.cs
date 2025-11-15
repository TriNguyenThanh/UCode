using AssignmentService.Application.Interfaces.Repositories;
using AssignmentService.Application.Interfaces.Services;
using AssignmentService.Application.DTOs.Responses;
using AssignmentService.Application.DTOs.Common;
using AssignmentService.Application.DTOs.Requests;
using AssignmentService.Domain.Entities;
using AssignmentService.Domain.Enums;
using System.Data.Common;

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
        try
        {
            assignment.AssignmentId = Guid.NewGuid();
            assignment.AssignedAt = DateTime.UtcNow;

            var createdAssignment = await _assignmentRepository.AddAsync(assignment);

            // Compute MaxScore from AssignmentProblems
            var maxScore = await _assignmentRepository.GetAssignmentMaxScoreAsync(createdAssignment.AssignmentId);

            // Fetch class roster from UserService and create AssignmentUsers 
            var userIds = await _userServiceClient.GetUserIdsByClassIdAsync(assignment.ClassId);
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
        catch (DbException ex)
        {
            throw new ApiException($"Database error while creating assignment: {ex.Message}", 500);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error creating assignment: {ex.Message}", 500);
        }
    }

    public async Task<Assignment> UpdateAssignmentAsync(Assignment assignment)
    {
        try
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

                var userIds = await _userServiceClient.GetUserIdsByClassIdAsync(updated.ClassId);
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
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (DbException ex)
        {
            throw new ApiException($"Database error while updating assignment: {ex.Message}", 500);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error updating assignment: {ex.Message}", 500);
        }
    }

    public async Task<bool> DeleteAssignmentAsync(Guid assignmentId)
    {
        try
        {
            var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
            if (assignment == null)
                return false;

            await _assignmentRepository.RemoveAsync(assignment.AssignmentId);
            return true;
        }
        catch (DbException ex)
        {
            throw new ApiException($"Database error while deleting assignment: {ex.Message}", 500);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error deleting assignment: {ex.Message}", 500);
        }
    }

    public async Task<Assignment?> GetAssignmentByIdAsync(Guid assignmentId)
    {
        try
        {
            return await _assignmentRepository.GetByIdWithDetailsAsync(assignmentId);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving assignment: {ex.Message}", 500);
        }
    }

    public async Task<Assignment?> GetAssignmentWithProblemBasicsAsync(Guid assignmentId)
    {
        try
        {
            return await _assignmentRepository.GetByIdWithProblemBasicsAsync(assignmentId);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving assignment with problem basics: {ex.Message}", 500);
        }
    }


    public async Task<List<Assignment>> GetAssignmentsByTeacherAsync(Guid teacherId)
    {
        try
        {
            return await _assignmentRepository.GetByTeacherIdAsync(teacherId);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving assignments for teacher: {ex.Message}", 500);
        }
    }

    public async Task<List<Assignment>> GetAssignmentsByStudentAsync(Guid studentId)
    {
        try
        {
            var assignments = await _assignmentRepository.GetByUserIdAsync(studentId);
            return assignments.Where(a => a.Status != AssignmentStatus.DRAFT).ToList();
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving assignments for student: {ex.Message}", 500);
        }
    }

    public async Task<List<Assignment>> GetAssignmentsByClassIdAsync(Guid classId)
    {
        try
        {
            return await _assignmentRepository.GetByClassIdAsync(classId);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving assignments for class: {ex.Message}", 500);
        }
    }

    public async Task<AssignmentUser?> GetAssignmentUserAsync(Guid assignmentId, Guid userId)
    {
        try
        {
            return await _assignmentRepository.GetAssignmentUserAsync(assignmentId, userId);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving assignment user: {ex.Message}", 500);
        }
    }

    public async Task<AssignmentUser?> GetAssignmentUserByIdAsync(Guid assignmentUserId)
    {
        try
        {
            return await _assignmentRepository.GetAssignmentUserByIdAsync(assignmentUserId);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving assignment user by ID: {ex.Message}", 500);
        }
    }

    public async Task<List<AssignmentUser>> GetAssignmentUsersAsync(Guid assignmentId)
    {
        try
        {
            return await _assignmentRepository.GetAssignmentUsersByAssignmentAsync(assignmentId);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving assignment users: {ex.Message}", 500);
        }
    }

    public async Task<AssignmentUser> UpdateAssignmentUserAsync(AssignmentUser detail)
    {
        try
        {
            return await _assignmentRepository.UpdateAssignmentUserAsync(detail);
        }
        catch (DbException ex)
        {
            throw new ApiException($"Database error while updating assignment user: {ex.Message}", 500);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error updating assignment user: {ex.Message}", 500);
        }
    }

    public async Task<AssignmentUser> UpdateAssignmentUserScoreAsync(Guid assignmentId, Guid userId, int score)
    {
        try
        {
            var assignmentUser = await _assignmentRepository.GetAssignmentUserAsync(assignmentId, userId);
            if (assignmentUser == null)
                throw new KeyNotFoundException("AssignmentUser not found");

            assignmentUser.Score += score;
            assignmentUser.Status = AssignmentUserStatus.SUBMITTED;
            Console.WriteLine($"[x] Updated AssignmentUser Score: {assignmentUser.Score}");

            return await _assignmentRepository.UpdateAssignmentUserAsync(assignmentUser);
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (DbException ex)
        {
            throw new ApiException($"Database error while updating assignment user score: {ex.Message}", 500);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error updating assignment user score: {ex.Message}", 500);
        }
    }

    public async Task<AssignmentStatistics> GetAssignmentStatisticsAsync(Guid assignmentId)
    {
        try
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
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error calculating assignment statistics: {ex.Message}", 500);
        }
    }

    // BestSubmission methods
    // public async Task<BestSubmission> SaveSubmissionAsync(BestSubmission submission)
    // {
    //     // submission.SubmissionId = Guid.NewGuid();
    //     // submission.Status = Domain.Enums.BestSubmissionStatus.NOT_STARTED;
    //     // submission.AttemptCount = 0;
    //     var existSubmission = await _assignmentRepository.GetSubmissionByIdAsync(submission.SubmissionId);
    //     if (existSubmission != null)
    //         return await _assignmentRepository.UpdateSubmissionAsync(submission);
    //     else
    //         return await _assignmentRepository.AddSubmissionAsync(submission);
    // }

    // public async Task<BestSubmission> UpdateSubmissionAsync(BestSubmission submission)
    // {
    //     return await _assignmentRepository.UpdateSubmissionAsync(submission);
    // }

    // public async Task<bool> DeleteSubmissionAsync(Guid submissionId)
    // {
    //     return await _assignmentRepository.DeleteSubmissionAsync(submissionId);
    // }

    // public async Task<BestSubmission?> GetSubmissionAsync(Guid assignmentUserId, Guid problemId)
    // {
    //     return await _assignmentRepository.GetSubmissionAsync(assignmentUserId, problemId);
    // }

    // public async Task<List<BestSubmission>> GetSubmissionsByAssignmentUserAsync(Guid assignmentUserId)
    // {
    //     return await _assignmentRepository.GetSubmissionsByAssignmentUserAsync(assignmentUserId);
    // }

    // public async Task<List<BestSubmission>> GetSubmissionsByAssignmentAsync(Guid assignmentId)
    // {
    //     return await _assignmentRepository.GetSubmissionsByAssignmentAsync(assignmentId);
    // }

    // public async Task<List<BestSubmission>> CreateSubmissionsForAssignmentUserAsync(Guid assignmentUserId, List<Guid> problemIds)
    // {

    //     throw new NotSupportedException("Cái method này để dành cho Trí làm nhé");
    //     // var submissions = new List<BestSubmission>();

    //     // // Lấy AssignmentUser để có AssignmentId
    //     // var assignmentUser = await _assignmentRepository.GetAssignmentUserByIdAsync(assignmentUserId);
    //     // if (assignmentUser == null)
    //     //     throw new KeyNotFoundException("AssignmentUser not found");

    //     // foreach (var problemId in problemIds)
    //     // {
    //     //     var submission = new BestSubmission
    //     //     {
    //     //         SubmissionId = Guid.NewGuid(),
    //     //         AssignmentUserId = assignmentUserId,
    //     //         ProblemId = problemId,
    //     //         Status = Domain.Enums.BestSubmissionStatus.NOT_STARTED,
    //     //         AttemptCount = 0
    //     //     };

    //     //     submissions.Add(submission);
    //     // }

    //     // return await _assignmentRepository.AddSubmissionsAsync(submissions);
    // }

    public async Task<AssignmentProblem?> GetAssignmentProblemAsync(Guid assignmentId, Guid problemId)
    {
        try
        {
            return await _assignmentRepository.GetAssignmentProblemAsync(assignmentId, problemId);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving assignment problem: {ex.Message}", 500);
        }
    }

    public async Task<Guid?> GetAssignmentOwnerIdAsync(Guid assignmentId)
    {
        try
        {
            return await _assignmentRepository.GetAssignmentOwnerIdAsync(assignmentId);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving assignment owner ID: {ex.Message}", 500);
        }
    }

    public async Task<(bool exists, Guid? ownerId)> CheckAssignmentExistsAndGetOwnerAsync(Guid assignmentId)
    {
        try
        {
            return await _assignmentRepository.CheckAssignmentExistsAndGetOwnerAsync(assignmentId);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error checking assignment existence: {ex.Message}", 500);
        }
    }

    public async Task<int> SyncStudentsToClassAssignmentsAsync(Guid classId, List<Guid> studentIds)
    {
        try
        {
            // Lấy tất cả assignments của class đang PUBLISHED hoặc Nháp (chưa CLOSED)
            var assignments = await _assignmentRepository.GetByClassIdAsync(classId);
            var activeAssignments = assignments
                .Where(a => a.Status == AssignmentStatus.PUBLISHED || a.Status == AssignmentStatus.DRAFT)
                .ToList();

            if (!activeAssignments.Any() || !studentIds.Any())
                return 0;

            int totalCreated = 0;

            foreach (var assignment in activeAssignments)
            {
                // Lấy danh sách studentId đã có AssignmentUser
                var existingAssignmentUsers = await _assignmentRepository.GetAssignmentUsersByAssignmentAsync(assignment.AssignmentId);
                var existingStudentIds = existingAssignmentUsers.Select(au => au.UserId).ToHashSet();

                // Chỉ thêm những student chưa có AssignmentUser
                var newStudentIds = studentIds.Where(sid => !existingStudentIds.Contains(sid)).ToList();

                if (newStudentIds.Any())
                {
                    var maxScore = await _assignmentRepository.GetAssignmentMaxScoreAsync(assignment.AssignmentId);
                    
                    var newAssignmentUsers = newStudentIds.Select(studentId => new AssignmentUser
                    {
                        AssignmentUserId = Guid.NewGuid(),
                        AssignmentId = assignment.AssignmentId,
                        UserId = studentId,
                        Status = AssignmentUserStatus.NOT_STARTED,
                        AssignedAt = DateTime.UtcNow,
                        MaxScore = maxScore
                    }).ToList();

                    await _assignmentRepository.AddAssignmentUsersAsync(newAssignmentUsers);
                    totalCreated += newAssignmentUsers.Count;
                }
            }

            return totalCreated;
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error syncing students to class assignments: {ex.Message}", 500);
        }
    }

    public async Task<AssignmentUser> IncrementTabSwitchCountAsync(Guid assignmentId, Guid userId)
    {
        try
        {
            var assignmentUser = await _assignmentRepository.GetAssignmentUserAsync(assignmentId, userId);
            if (assignmentUser == null)
                throw new ApiException("Assignment detail not found", 404);

            assignmentUser.TabSwitchCount++;
            await _assignmentRepository.UpdateAssignmentUserAsync(assignmentUser);
            
            return assignmentUser;
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error incrementing tab switch count: {ex.Message}", 500);
        }
    }

    public async Task<AssignmentUser> IncrementCapturedAICountAsync(Guid assignmentId, Guid userId)
    {
        try
        {
            var assignmentUser = await _assignmentRepository.GetAssignmentUserAsync(assignmentId, userId);
            if (assignmentUser == null)
                throw new ApiException("Assignment detail not found", 404);

            assignmentUser.CapturedAICount++;
            await _assignmentRepository.UpdateAssignmentUserAsync(assignmentUser);
            
            return assignmentUser;
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error incrementing AI detection count: {ex.Message}", 500);
        }
    }

    public async Task<bool> LogExamActivityAsync(Guid assignmentId, Guid userId, ActivityLogRequest activityLog)
    {
        try
        {
            var assignmentUser = await _assignmentRepository.GetAssignmentUserAsync(assignmentId, userId);
            if (assignmentUser == null)
                return false;

            var activity = new ExamActivityLog
            {
                ActivityLogId = Guid.NewGuid(),
                AssignmentUserId = assignmentUser.AssignmentUserId,
                ActivityType = activityLog.ActivityType,
                Timestamp = activityLog.Timestamp,
                Metadata = activityLog.Metadata,
                SuspicionLevel = activityLog.SuspicionLevel
            };

            await _assignmentRepository.AddExamActivityLogAsync(activity);
            return true;
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error logging exam activity: {ex.Message}", 500);
        }
    }

    public async Task<int> LogExamActivitiesBatchAsync(Guid assignmentId, Guid userId, List<ActivityLogRequest> activities)
    {
        try
        {
            if (!activities.Any())
                return 0;

            var assignmentUser = await _assignmentRepository.GetAssignmentUserAsync(assignmentId, userId);
            if (assignmentUser == null)
                return 0;

            var activityLogs = activities.Select(a => new ExamActivityLog
            {
                ActivityLogId = Guid.NewGuid(),
                AssignmentUserId = assignmentUser.AssignmentUserId,
                ActivityType = a.ActivityType,
                Timestamp = a.Timestamp,
                Metadata = a.Metadata,
                SuspicionLevel = a.SuspicionLevel
            }).ToList();

            await _assignmentRepository.AddExamActivityLogsBatchAsync(activityLogs);
            return activityLogs.Count;
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error logging exam activities batch: {ex.Message}", 500);
        }
    }
}
