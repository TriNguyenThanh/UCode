using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using AssignmentService.Api.Middlewares;
using AssignmentService.Application.DTOs.Common;
using AssignmentService.Application.DTOs.Requests;
using AssignmentService.Application.DTOs.Responses;
using AssignmentService.Application.Interfaces.Services;
using AssignmentService.Domain.Entities;

namespace AssignmentService.Api.Controllers;

/// <summary>
/// Controller for managing assignments in the ucode.io.vn platform
/// </summary>
[ApiController]
[Route("api/v1/assignments")]
[ValidateUserId]
public class AssignmentController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;
    private readonly IMapper _mapper;

    public AssignmentController(IAssignmentService assignmentService, IMapper mapper)
    {
        _assignmentService = assignmentService;
        _mapper = mapper;
    }

    #region Helper Methods
    
    /// <summary>
    /// Gets authenticated user ID from X-User-Id header
    /// </summary>
    private Guid GetAuthenticatedUserId()
    {
        var userId = HttpContext.Items["X-User-Id"]?.ToString();
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userIdGuid))
            throw new ApiException("X-User-Id header is missing or invalid");
        
        return userIdGuid;
    }

    /// <summary>
    /// Verifies ownership using lightweight query (only fetches AssignedBy)
    /// </summary>
    private async Task VerifyAssignmentOwnershipLightweightAsync(Guid assignmentId, Guid userId)
    {
        var ownerId = await _assignmentService.GetAssignmentOwnerIdAsync(assignmentId);
        if (ownerId == null)
            throw new ApiException("Assignment not found");
        
        if (ownerId.Value != userId)
            throw new ApiException("You do not have permission to access this assignment");
    }

    #endregion

    /// <summary>
    /// Creates a new assignment
    /// </summary>
    /// <param name="request">Assignment creation request containing assignment details</param>
    /// <returns>Created assignment information</returns>
    /// <response code="200">Assignment created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized - Teacher role required</response>
    /// <response code="422">Validation errors</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("create")]
    [RequireRole("teacher,admin")]
    [ProducesResponseType(typeof(ApiResponse<AssignmentResponse>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ValidationErrorResponse), 422)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> CreateAssignment([FromBody] AssignmentRequest request)
    {
        var userId = Guid.Parse(HttpContext.Items["X-User-Id"]?.ToString()!);
        
        var assignment = _mapper.Map<Assignment>(request);
        assignment.AssignedBy = userId;
        
        var created = await _assignmentService.CreateAssignmentAsync(assignment);
        var response = _mapper.Map<AssignmentResponse>(created);
        
        return Ok(ApiResponse<AssignmentResponse>.SuccessResponse(response, "Assignment created successfully"));
    }

    /// <summary>
    /// Updates an existing assignment
    /// </summary>
    /// <param name="id">The unique identifier of the assignment to update</param>
    /// <param name="request">Assignment update request containing updated assignment details</param>
    /// <returns>Updated assignment information</returns>
    /// <response code="200">Assignment updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized - Teacher role required</response>
    /// <response code="403">Forbidden - You don't have permission to update this assignment</response>
    /// <response code="404">Assignment not found</response>
    /// <response code="422">Validation errors</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("update/{id:guid}")]
    [RequireRole("teacher,admin")]
    [ProducesResponseType(typeof(ApiResponse<AssignmentResponse>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ForbiddenErrorResponse), 403)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ValidationErrorResponse), 422)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> UpdateAssignment(Guid id, [FromBody] AssignmentRequest request)
    {
        var userId = GetAuthenticatedUserId();
        
        // Use lightweight query - only check ownership, don't need full Assignment entity
        await VerifyAssignmentOwnershipLightweightAsync(id, userId);
        
        var entity = _mapper.Map<Assignment>(request);
        entity.AssignmentId = id;
        var updated = await _assignmentService.UpdateAssignmentAsync(entity);
        var response = _mapper.Map<AssignmentResponse>(updated);
        
        return Ok(ApiResponse<AssignmentResponse>.SuccessResponse(response, "Assignment updated successfully"));
    }

    /// <summary>
    /// Deletes an assignment by ID
    /// </summary>
    /// <param name="id">The unique identifier of the assignment to delete</param>
    /// <returns>Success confirmation</returns>
    /// <response code="200">Assignment deleted successfully</response>
    /// <response code="401">Unauthorized - Teacher role required</response>
    /// <response code="403">Forbidden - You don't have permission to delete this assignment</response>
    /// <response code="404">Assignment not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("delete/{id:guid}")]
    [RequireRole("teacher,admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ForbiddenErrorResponse), 403)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> DeleteAssignment(Guid id)
    {
        var userId = GetAuthenticatedUserId();
        
        // Use lightweight query - only check ownership, don't need full Assignment entity
        await VerifyAssignmentOwnershipLightweightAsync(id, userId);
        
        await _assignmentService.DeleteAssignmentAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(new {}, "Assignment deleted successfully"));
    }

    // /// <summary>
    // /// Retrieves a specific assignment by ID with statistics
    // /// </summary>
    // /// <param name="id">The unique identifier of the assignment</param>
    // /// <returns>Assignment details with statistics</returns>
    // /// <response code="200">Assignment retrieved successfully</response>
    // /// <response code="401">Unauthorized - Teacher role required</response>
    // /// <response code="404">Assignment not found</response>
    // /// <response code="500">Internal server error</response>
    // [HttpGet("{id:guid}")]
    // [ProducesResponseType(typeof(ApiResponse<AssignmentResponse>), 200)]
    // [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    // [ProducesResponseType(typeof(ErrorResponse), 404)]
    // [ProducesResponseType(typeof(ErrorResponse), 500)]
    // public async Task<IActionResult> GetAssignment(Guid id)
    // {
    //     var assignment = await _assignmentService.GetAssignmentByIdAsync(id);
    //     if (assignment == null)
    //         return NotFound(ApiResponse<AssignmentResponse>.ErrorResponse("Assignment not found"));

    //     var response =  _mapper.Map<AssignmentResponse>(assignment);
    //     response.Statistics = await _assignmentService.GetAssignmentStatisticsAsync(id);

    //     return Ok(ApiResponse<AssignmentResponse>.SuccessResponse(response));
    // }

    /// <summary>
    /// Retrieves assignment with only basic problem info (ID, Title, Code, Difficulty)
    /// </summary>
    /// <param name="id">The unique identifier of the assignment</param>
    /// <returns>Assignment with problem basics only</returns>
    /// <response code="200">Assignment retrieved successfully</response>
    /// <response code="404">Assignment not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:guid}")]
    // [RequireRole("teacher,admin")]
    [ProducesResponseType(typeof(ApiResponse<AssignmentResponse>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> GetAssignmentWithBasics(Guid id)
    {
        var assignment = await _assignmentService.GetAssignmentWithProblemBasicsAsync(id);
        if (assignment == null)
            return NotFound(ApiResponse<AssignmentResponse>.ErrorResponse("Assignment not found"));
        
        var response = _mapper.Map<AssignmentResponse>(assignment);
        response.TotalProblems = response.Problems?.Count;

        return Ok(ApiResponse<AssignmentResponse>.SuccessResponse(response, "Assignment with problem basics retrieved successfully"));
    }

    /// <summary>
    /// Retrieves all assignments created by the current teacher
    /// </summary>
    /// <returns>List of assignments owned by the current teacher</returns>
    /// <response code="200">Assignments retrieved successfully</response>
    /// <response code="401">Unauthorized - Teacher role required</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("my-assignments")]
    [RequireRole("teacher,admin")]
    [ProducesResponseType(typeof(ApiResponse<List<AssignmentResponse>>), 200)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> GetMyAssignments()
    {
        var userId = Guid.Parse(HttpContext.Items["X-User-Id"]?.ToString()!);
        var assignments = await _assignmentService.GetAssignmentsByTeacherAsync(userId);
        var response = _mapper.Map<List<AssignmentResponse>>(assignments);
        
        return Ok(ApiResponse<List<AssignmentResponse>>.SuccessResponse(response));
    }

    /// <summary>
    /// Retrieves all students and their status for a specific assignment
    /// </summary>
    /// <param name="id">The unique identifier of the assignment</param>
    /// <returns>List of assignment details for all students</returns>
    /// <response code="200">Assignment students retrieved successfully</response>
    /// <response code="401">Unauthorized - Teacher role required</response>
    /// <response code="404">Assignment not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:guid}/students")]
    [RequireRole("teacher,admin")]
    [ProducesResponseType(typeof(ApiResponse<List<AssignmentUserDto>>), 200)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> GetAssignmentStudents(Guid id)
    {
        var details = await _assignmentService.GetAssignmentUsersAsync(id);
        var response = _mapper.Map<List<AssignmentUserDto>>(details);
        
        return Ok(ApiResponse<List<AssignmentUserDto>>.SuccessResponse(response));
    }

    /// <summary>
    /// Retrieves statistics for a specific assignment
    /// </summary>
    /// <param name="id">The unique identifier of the assignment</param>
    /// <returns>Assignment statistics including completion rates and scores</returns>
    /// <response code="200">Assignment statistics retrieved successfully</response>
    /// <response code="401">Unauthorized - Teacher role required</response>
    /// <response code="404">Assignment not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:guid}/statistics")]
    [RequireRole("teacher,admin")]
    [ProducesResponseType(typeof(ApiResponse<AssignmentStatistics>), 200)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> GetAssignmentStatistics(Guid id)
    {
        var stats = await _assignmentService.GetAssignmentStatisticsAsync(id);
        return Ok(ApiResponse<AssignmentStatistics>.SuccessResponse(stats));
    }

    /// <summary>
    /// Retrieves all assignments assigned to the current student
    /// </summary>
    /// <returns>List of assignments assigned to the current student</returns>
    /// <response code="200">Student assignments retrieved successfully</response>
    /// <response code="401">Unauthorized - Student role required</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("student/my-assignments")]
    [RequireRole("student")]
    [ProducesResponseType(typeof(ApiResponse<List<AssignmentResponse>>), 200)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> GetStudentAssignments()
    {
        var userId = Guid.Parse(HttpContext.Items["X-User-Id"]?.ToString()!);
        var assignments = await _assignmentService.GetAssignmentsByStudentAsync(userId);
        var response = _mapper.Map<List<AssignmentResponse>>(assignments);

        return Ok(ApiResponse<List<AssignmentResponse>>.SuccessResponse(response));
    }
    
    [HttpGet("class/{classId:guid}")]
    // [RequireRole("student,teacher")]
    [ProducesResponseType(typeof(ApiResponse<List<AssignmentResponse>>), 200)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> GetAssignmentsByClass(Guid classId)
    {
        var userId = GetAuthenticatedUserId();
        var assignments = await _assignmentService.GetAssignmentsByClassIdAsync(classId);

        assignments = assignments.Where(a =>
            a.AssignedBy == userId).ToList();

        var response = _mapper.Map<List<AssignmentResponse>>(assignments);
        
        return Ok(ApiResponse<List<AssignmentResponse>>.SuccessResponse(response));
    }

    /// <summary>
    /// Retrieves assignment detail for the current student
    /// </summary>
    /// <param name="id">The unique identifier of the assignment</param>
    /// <returns>Assignment detail for the current student</returns>
    /// <response code="200">Assignment detail retrieved successfully</response>
    /// <response code="401">Unauthorized - Student role required</response>
    /// <response code="404">Assignment detail not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:guid}/student/my-detail")]
    [RequireRole("student")]
    [ProducesResponseType(typeof(ApiResponse<AssignmentUserDto>), 200)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> GetMyAssignmentUser(Guid id)
    {
        var userId = Guid.Parse(HttpContext.Items["X-User-Id"]?.ToString()!);
        var detail = await _assignmentService.GetAssignmentUserAsync(id, userId);
        
        if (detail == null)
            return NotFound(ApiResponse<AssignmentUserDto>.ErrorResponse("Assignment detail not found"));
        
        var response = _mapper.Map<AssignmentUserDto>(detail);
        return Ok(ApiResponse<AssignmentUserDto>.SuccessResponse(response));
    }

    /// <summary>
    /// Updates assignment status for the current student (Internal API - Backend communication)
    /// </summary>
    /// <param name="id">The unique identifier of the assignment</param>
    /// <param name="request">Assignment detail update request</param>
    /// <returns>Updated assignment detail</returns>
    /// <response code="200">Assignment status updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized - Student role required</response>
    /// <response code="404">Assignment detail not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id:guid}/student/update-status")]
    [RequireRole("student")]
    [ProducesResponseType(typeof(ApiResponse<AssignmentUserDto>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> UpdateAssignmentStatus(Guid id, [FromBody] AssignmentUserDto request)
    {
        var userId = Guid.Parse(HttpContext.Items["X-User-Id"]?.ToString()!);
        var detail = await _assignmentService.GetAssignmentUserAsync(id, userId);
        
        if (detail == null)
            return NotFound(ApiResponse<AssignmentUserDto>.ErrorResponse("Assignment detail not found"));
        
        // Update fields
        detail.Status = request.Status;
        // StudentNote moved to per-problem scope; ignore at assignment level
        
        if (request.Status == Domain.Enums.AssignmentUserStatus.IN_PROGRESS && !detail.StartedAt.HasValue)
            detail.StartedAt = DateTime.UtcNow;
        
        // SUBMITTED timestamp and submission count are tracked per problem now
        
        var updated = await _assignmentService.UpdateAssignmentUserAsync(detail);
        var response = _mapper.Map<AssignmentUserDto>(updated);
        
        return Ok(ApiResponse<AssignmentUserDto>.SuccessResponse(response, "Status updated successfully"));
    }

    /// <summary>
    /// Starts an assignment for the current student
    /// </summary>
    /// <param name="id">The unique identifier of the assignment</param>
    /// <returns>Updated assignment detail with started status</returns>
    /// <response code="200">Assignment started successfully</response>
    /// <response code="400">Assignment already started</response>
    /// <response code="401">Unauthorized - Student role required</response>
    /// <response code="404">Assignment detail not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{id:guid}/student/start")]
    [RequireRole("student")]
    [ProducesResponseType(typeof(ApiResponse<AssignmentUserDto>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> StartAssignment(Guid id)
    {
        var userId = Guid.Parse(HttpContext.Items["X-User-Id"]?.ToString()!);
        var detail = await _assignmentService.GetAssignmentUserAsync(id, userId);
        if (detail == null)
            return NotFound(ApiResponse<AssignmentUserDto>.ErrorResponse("Assignment detail not found"));
        
        if (detail.Status != Domain.Enums.AssignmentUserStatus.NOT_STARTED)
            return BadRequest(ApiResponse<AssignmentUserDto>.ErrorResponse("Assignment already started"));
        
        detail.Status = Domain.Enums.AssignmentUserStatus.IN_PROGRESS;
        detail.StartedAt = DateTime.UtcNow;
        var updated = await _assignmentService.UpdateAssignmentUserAsync(detail);
        var response = _mapper.Map<AssignmentUserDto>(updated);
        return Ok(ApiResponse<AssignmentUserDto>.SuccessResponse(response, "Assignment started successfully"));
    }

    // /// <summary>
    // /// Retrieves all submissions for the current student in a specific assignment
    // /// </summary>
    // /// <param name="id">The unique identifier of the assignment</param>
    // /// <returns>List of submissions for the current student</returns>
    // /// <response code="200">Submissions retrieved successfully</response>
    // /// <response code="401">Unauthorized - Student role required</response>
    // /// <response code="404">Assignment detail not found</response>
    // /// <response code="500">Internal server error</response>
    // [HttpGet("{id:guid}/student/my-submissions")]
    // [RequireRole("student")]
    // [ProducesResponseType(typeof(ApiResponse<List<BestSubmissionDto>>), 200)]
    // [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    // [ProducesResponseType(typeof(ErrorResponse), 404)]
    // [ProducesResponseType(typeof(ErrorResponse), 500)]
    // public async Task<IActionResult> GetMySubmissions(Guid id)
    // {
    //     var userId = Guid.Parse(HttpContext.Items["X-User-Id"]?.ToString()!);
    //     var detail = await _assignmentService.GetAssignmentUserAsync(id, userId);
        
    //     if (detail == null)
    //         return NotFound(ApiResponse<List<BestSubmissionDto>>.ErrorResponse("Assignment detail not found"));
        
    //     var submissions = await _assignmentService.GetSubmissionsByAssignmentUserAsync(detail.AssignmentUserId);
    //     var response = _mapper.Map<List<BestSubmissionDto>>(submissions);
        
    //     return Ok(ApiResponse<List<BestSubmissionDto>>.SuccessResponse(response));
    // }

    // ///================[ PHẦN NÀY THUỘC PHẦN CỦA SUBMISSION SERVICE RỒI, CHỈ ĐỂ TEST THÔI ]

    /// <summary>
    /// Webhook endpoint for Submission Service to save assignment problem submission results
    /// </summary>
    /// <param name="assignmentId">The unique identifier of the assignment</param>
    /// <param name="request">Submission result containing solution code, test results, and execution metrics</param>
    /// <returns>Saved submission information with calculated score</returns>
    /// <response code="200">Submission saved successfully with calculated score</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized - api-key webhook required</response>
    /// <response code="404">Assignment detail not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("webhook/{assignmentId:guid}/save-assignment-problem-submission")]
    [ProducesResponseType(typeof(ApiResponse<BestSubmissionResponse>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public Task<IActionResult> SaveAssignmentProblemSubmission(Guid assignmentId, [FromBody] BestSubmissionResponse request)
    {
        // try {

    //     //     var userId = request.UserId ?? Guid.Parse(HttpContext.Items["X-User-Id"]?.ToString()!);

    //     //     var apiKey = Request.Headers["X-Api-Key"].ToString();
    //     //     if (/* apiKey != Environment.GetEnvironmentVariable("API_KEY_WEBHOOK") || */ apiKey != "123456789")
    //     //         throw new ApiException("Unauthorized - api-key webhook required");
            
    //     //     var problemId = request.ProblemId;
    //     //     if (request.SubmissionId == null){
    //     //         throw new ApiException("Submission ID is required");
    //     //     }

    //     //     var detail = await _assignmentService.GetAssignmentUserAsync(assignmentId, userId);
    //     //     if (detail == null)
    //     //         throw new ApiException("Assignment detail not found");
            
    //     //     // Đếm số submissions hiện tại cho problem này để tính attempt count
    //     //     var existingSubmissions = await _assignmentService.GetSubmissionsByAssignmentUserAsync(detail.AssignmentUserId);

    //     //     var problemSubmissions = existingSubmissions?.Where(s => s.ProblemId == problemId).ToList() ?? new List<AssignmentProblemSubmission>();
            
    //     //     // Lấy thông tin assignment problem để có MaxScore
    //     //     var assignmentProblem = await _assignmentService.GetAssignmentProblemAsync(assignmentId, problemId);
    //     //     if (assignmentProblem == null)
    //     //         throw new ApiException("Assignment problem not found");
            
    //     //     // Tính toán score dựa trên số test cases đã pass và MaxScore từ assignment_problem
    //     //     var totalTestCases = request.TotalTestCases ?? 0;
    //     //     var passedTestCases = request.PassedTestCases ?? 0;
    //     //     var maxScore = assignmentProblem.Points; 
    //     //     var score = totalTestCases > 0 ? (int)Math.Round((double)passedTestCases / totalTestCases * maxScore) : 0;
            
    //     //     // Xác định status dựa trên score
    //     //     var status = score == maxScore ? Domain.Enums.AssignmentProblemSubmissionStatus.ACCEPTED : 
    //     //                 score > 0 ? Domain.Enums.AssignmentProblemSubmissionStatus.PARTIAL_ACCEPTED : 
    //     //                 Domain.Enums.AssignmentProblemSubmissionStatus.WRONG_ANSWER;

    //     //     // Tạo submission mới mỗi lần submit
    //     //     var submission = new AssignmentProblemSubmission
    //     //     {
    //     //         SubmissionId = request.SubmissionId.Value,
    //     //         AssignmentUserId = detail.AssignmentUserId,
    //     //         ProblemId = problemId,
    //     //         SolutionCode = request.SolutionCode,
    //     //         Status = status,
    //     //         SubmittedAt = (DateTime)request.SubmittedAt,
    //     //         AttemptCount = problemSubmissions.Count + 1,
    //     //         Score = score,
    //     //         MaxScore = maxScore,
    //     //         ExecutionTime = request.ExecutionTime,
    //     //         MemoryUsed = request.MemoryUsed
    //     //     };
            
            

    //     //     var created = await _assignmentService.SaveSubmissionAsync(submission);
    //     //     var response = _mapper.Map<BestSubmissionDto>(created);
            
        // return Ok(ApiResponse<BestSubmissionDto>.SuccessResponse(response, "Submission saved successfully"));
        // } catch (Exception e) {
        //     Console.Write(e);
        //     return BadRequest(ApiResponse<BestSubmissionDto>.ErrorResponse(e.Message));

    //     // }

        return Task.FromResult<IActionResult>(BadRequest(ApiResponse<BestSubmissionResponse>.ErrorResponse("Api này chưa có đâu nhé")));
    }

    // /// <summary>
    // /// Retrieves all submissions for a specific assignment (Teacher only)
    // /// </summary>
    // /// <param name="id">The unique identifier of the assignment</param>
    // /// <returns>List of all submissions for the assignment</returns>
    // /// <response code="200">All submissions retrieved successfully</response>
    // /// <response code="401">Unauthorized - Teacher role required</response>
    // /// <response code="403">Forbidden - You don't have permission to view this assignment's submissions</response>
    // /// <response code="404">Assignment not found</response>
    // /// <response code="500">Internal server error</response>
    // [HttpGet("{id:guid}/all-submissions")]
    // [RequireRole("teacher,admin")]
    // [ProducesResponseType(typeof(ApiResponse<List<BestSubmissionDto>>), 200)]
    // [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    // [ProducesResponseType(typeof(ForbiddenErrorResponse), 403)]
    // [ProducesResponseType(typeof(ErrorResponse), 404)]
    // [ProducesResponseType(typeof(ErrorResponse), 500)]
    // public async Task<IActionResult> GetAllSubmissions(Guid id)
    // {
    //     var userId = GetAuthenticatedUserId();
        
    //     // Use lightweight query - only check ownership, don't need full Assignment entity
    //     await VerifyAssignmentOwnershipLightweightAsync(id, userId);
        
    //     var submissions = await _assignmentService.GetSubmissionsByAssignmentAsync(id);
    //     var response = _mapper.Map<List<BestSubmissionDto>>(submissions);
        
    //     return Ok(ApiResponse<List<BestSubmissionDto>>.SuccessResponse(response));
    // }

    /// <summary>
    /// Grades a specific submission (Teacher only)
    /// </summary>
    /// <param name="id">The unique identifier of the assignment</param>
    /// <param name="submissionId">The unique identifier of the submission to grade</param>
    /// <param name="score">The score to assign to the submission</param>
    /// <returns>Updated submission with grade information</returns>
    /// <response code="200">Submission graded successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized - Teacher role required</response>
    /// <response code="403">Forbidden - You don't have permission to grade this assignment</response>
    /// <response code="404">Assignment or submission not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id:guid}/grade-submission/{submissionId:guid}")]
    [RequireRole("teacher,admin")]
    [ProducesResponseType(typeof(ApiResponse<BestSubmissionResponse>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ForbiddenErrorResponse), 403)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> GradeSubmission([FromQuery]Guid id, [FromQuery]Guid submissionId, [FromBody] int score)
    {
        // var assignment = await _assignmentService.GetAssignmentByIdAsync(id);
        // if (assignment == null)
        //     return NotFound(ApiResponse<BestSubmissionResponse>.ErrorResponse("Assignment not found"));
        
        // var userId = GetAuthenticatedUserId();
        
    //     // // Find submission by ID (we need to add this method to service)
    //     // var submissions = await _assignmentService.GetSubmissionsByAssignmentAsync(id);
    //     // var submission = submissions.FirstOrDefault(s => s.SubmissionId == submissionId);
        
    //     // if (submission == null)
    //     //     return NotFound(ApiResponse<BestSubmissionDto>.ErrorResponse("Submission not found"));
        
    //     // // Update grade
    //     // submission.Score = request.Score;
    //     // submission.TeacherFeedback = request.TeacherFeedback;
    //     // submission.Status = Domain.Enums.AssignmentProblemSubmissionStatus.GRADED;
        
    //     // var updated = await _assignmentService.UpdateSubmissionAsync(submission);
    //     // var response = _mapper.Map<BestSubmissionDto>(updated);
        
    //     // return Ok(ApiResponse<BestSubmissionResponse>.SuccessResponse(response, "Submission graded successfully"));

        return BadRequest(ApiResponse<BestSubmissionResponse>.ErrorResponse("Api này chưa có đâu nhé"));
    }

    /// <summary>
    /// Syncs students to all active assignments of a class
    /// Called by User Service when students are added to a class
    /// Internal API - No user authentication required
    /// </summary>
    /// <param name="classId">The unique identifier of the class</param>
    /// <param name="request">List of student IDs to sync</param>
    /// <returns>Number of AssignmentUsers created</returns>
    /// <response code="200">Students synced successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("classes/{classId:guid}/students/sync")]
    [SkipValidateUserId]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> SyncStudentsToClassAssignments(Guid classId, [FromBody] SyncStudentsRequest request)
    {
        if (request.StudentIds == null || !request.StudentIds.Any())
            return BadRequest(ApiResponse<object>.ErrorResponse("StudentIds list cannot be empty"));

        var count = await _assignmentService.SyncStudentsToClassAssignmentsAsync(classId, request.StudentIds);
        
        return Ok(ApiResponse<object>.SuccessResponse(
            new { AssignmentUsersCreated = count },
            $"Synced {request.StudentIds.Count} student(s) to {count} assignment user(s)"
        ));
    }

    /// <summary>
    /// Increments the tab switch count for a student's assignment
    /// Called when student switches tabs or loses focus during an examination
    /// </summary>
    /// <param name="id">The unique identifier of the assignment</param>
    /// <returns>Updated assignment user with incremented tab switch count</returns>
    /// <response code="200">Tab switch count incremented successfully</response>
    /// <response code="401">Unauthorized - Student role required</response>
    /// <response code="404">Assignment detail not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{id:guid}/student/increment-tab-switch")]
    [RequireRole("student")]
    [ProducesResponseType(typeof(ApiResponse<AssignmentUserDto>), 200)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> IncrementTabSwitch(Guid id)
    {
        var userId = GetAuthenticatedUserId();
        var updated = await _assignmentService.IncrementTabSwitchCountAsync(id, userId);
        
        if (updated == null)
            return NotFound(ApiResponse<AssignmentUserDto>.ErrorResponse("Assignment detail not found"));
        
        var response = _mapper.Map<AssignmentUserDto>(updated);
        return Ok(ApiResponse<AssignmentUserDto>.SuccessResponse(response, "Tab switch recorded"));
    }

    /// <summary>
    /// Increments the AI detection count for a student's assignment
    /// Called when AI usage is detected during an examination
    /// </summary>
    /// <param name="id">The unique identifier of the assignment</param>
    /// <returns>Updated assignment user with incremented AI detection count</returns>
    /// <response code="200">AI detection count incremented successfully</response>
    /// <response code="401">Unauthorized - Student role required</response>
    /// <response code="404">Assignment detail not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{id:guid}/student/increment-ai-detection")]
    [RequireRole("student")]
    [ProducesResponseType(typeof(ApiResponse<AssignmentUserDto>), 200)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> IncrementAIDetection(Guid id)
    {
        var userId = GetAuthenticatedUserId();
        var updated = await _assignmentService.IncrementCapturedAICountAsync(id, userId);
        
        if (updated == null)
            return NotFound(ApiResponse<AssignmentUserDto>.ErrorResponse("Assignment detail not found"));
        
        var response = _mapper.Map<AssignmentUserDto>(updated);
        return Ok(ApiResponse<AssignmentUserDto>.SuccessResponse(response, "AI detection recorded"));
    }

    /// <summary>
    /// Logs a single activity event for exam monitoring
    /// </summary>
    /// <param name="id">The unique identifier of the assignment</param>
    /// <param name="request">Activity log data</param>
    /// <returns>Success confirmation</returns>
    /// <response code="200">Activity logged successfully</response>
    /// <response code="401">Unauthorized - Student role required</response>
    /// <response code="404">Assignment detail not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{id:guid}/student/log-activity")]
    [RequireRole("student")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> LogActivity(Guid id, [FromBody] ActivityLogRequest request)
    {
        var userId = GetAuthenticatedUserId();
        // var success = await _assignmentService.LogExamActivityAsync(id, userId, request);
        
        // if (!success)
        //     return NotFound(ApiResponse<object>.ErrorResponse("Assignment detail not found"));
        
        return Ok(ApiResponse<object>.SuccessResponse(new {}, "Activity logged"));
    }

    /// <summary>
    /// Logs multiple activity events in a batch for exam monitoring
    /// More efficient than logging one by one
    /// </summary>
    /// <param name="id">The unique identifier of the assignment</param>
    /// <param name="request">Batch of activity logs</param>
    /// <returns>Success confirmation with count of logged activities</returns>
    /// <response code="200">Activities logged successfully</response>
    /// <response code="401">Unauthorized - Student role required</response>
    /// <response code="404">Assignment detail not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{id:guid}/student/log-activities-batch")]
    [RequireRole("student")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> LogActivitiesBatch(Guid id, [FromBody] ActivityLogBatchRequest request)
    {
        var userId = GetAuthenticatedUserId();
        // var count = await _assignmentService.LogExamActivitiesBatchAsync(id, userId, request.Activities);
        
        // if (count == 0)
        //     return NotFound(ApiResponse<object>.ErrorResponse("Assignment detail not found or no activities to log"));
        
        return Ok(ApiResponse<object>.SuccessResponse(
            new {  },
            $"Logged  activities"
        ));
    }
}
