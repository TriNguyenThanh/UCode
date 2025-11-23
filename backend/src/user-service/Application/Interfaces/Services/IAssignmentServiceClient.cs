namespace UserService.Application.Interfaces.Services;

public interface IAssignmentServiceClient
{
    /// <summary>
    /// Syncs students to all active assignments of a class
    /// </summary>
    /// <param name="classId">Class ID</param>
    /// <param name="studentIds">List of student IDs to sync</param>
    /// <returns>Number of AssignmentUsers created</returns>
    Task<int> SyncStudentsToClassAssignmentsAsync(Guid classId, List<Guid> studentIds);

    /// <summary>
    /// Syncs delete user to assignment service
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Success</returns>
    Task<bool> SyncDeleteUserAsync(Guid userId);
}
