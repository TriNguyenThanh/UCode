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

    /// <summary>
    /// Tạo tài khoản cho sinh viên
    /// </summary>
    /// <param name="fullNames">Tên đầy đủ của sinh viên</param>
    /// <param name="emails">Danh sách email của sinh viên</param>
    /// <param name="password">Mật khẩu tạm thời của sinh viên</param>
    /// <returns>Success message</returns>
    /// <response code="200">Account created successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Failed to create account</response>
    Task<bool> SendCreatedAccountEmails(List<string> fullNames, List<string> emails, string password);

    /// <summary>
    /// Thêm 1 sinh viên vào lớp
    /// </summary>
    /// <param name="Emails">Danh sách email của sinh viên</param>
    /// <param name="ClassName">Tên lớp</param>
    /// <param name="TeacherName">Tên giảng viên</param>
    /// <param name="StartDate">Ngày bắt đầu lớp</param>
    /// <returns>Success message</returns>
    /// <response code="200">Account created successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Failed to create account</response>
    Task<bool> SendAddedToClassEmails(List<string> Emails, string ClassName, string TeacherName, DateTime StartDate);
}
