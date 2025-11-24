using Microsoft.AspNetCore.Mvc;
using AssignmentService.Application.Interfaces.Services;
using AssignmentService.Application.DTOs.Common;
using AssignmentService.Application.DTOs.Requests;

namespace AssignmentService.Api.Controllers;

/// <summary>
/// Controller for email service
/// </summary>
[ApiController]
[Route("api/v1/email")]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailController> _logger;

    public EmailController(IEmailService emailService, ILogger<EmailController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    // /// <summary>
    // /// Sending multiple emails in parallel (batch)
    // /// </summary>
    // /// <param name="emails">List of email addresses to send to</param>
    // /// <param name="subject">Email subject (optional)</param>
    // /// <param name="maxConcurrency">Maximum number of concurrent emails (default: 10)</param>
    // /// <returns>Success message</returns>
    // /// <response code="200">Emails sent successfully</response>
    // /// <response code="400">Invalid request</response>
    // /// <response code="500">Failed to send emails</response>
    // [HttpPost("send-batch")]
    // [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    // [ProducesResponseType(typeof(ErrorResponse), 400)]
    // [ProducesResponseType(typeof(ErrorResponse), 500)]
    // public async Task<IActionResult> SendBatchEmail(
    //     [FromBody] List<string> emails,
    //     [FromQuery] string? subject = null,
    //     [FromQuery] int maxConcurrency = 10)
    // {
    //     if (emails == null || emails.Count == 0)
    //     {
    //         return BadRequest(ApiResponse<object>.ErrorResponse("At least one email address is required"));
    //     }

    //     try
    //     {
    //         var _emailSubject = subject ?? "Batch Email from UCode";

    //         var emailMessages = emails.Select(email => new EmailQueueMessage
    //         {
    //             To = email,
    //             Subject = _emailSubject,
    //             HtmlContent = 
    //         }).ToList();

    //         await _emailService.SendBatchAsync(emailMessages, maxConcurrency);

    //         _logger.LogInformation("Batch test emails sent successfully to {Count} recipients", emails.Count);

    //         return Ok(ApiResponse<object?>.SuccessResponse(
    //             null,
    //             $"Batch test emails sent successfully to {emails.Count} recipients"
    //         ));
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Failed to send batch test emails");
    //         return StatusCode(500, ApiResponse<object>.ErrorResponse(
    //             $"Failed to send batch emails: {ex.Message}"
    //         ));
    //     }
    // }

    // /// <summary>
    // /// Test queuing an email (async processing via RabbitMQ)
    // /// </summary>
    // /// <param name="to">Recipient email address</param>
    // /// <param name="htmlContent">HTML content of the email (optional)</param>
    // /// <param name="subject">Email subject (optional)</param>
    // /// <returns>Success message</returns>
    // /// <response code="200">Email queued successfully</response>
    // /// <response code="400">Invalid email address</response>
    // /// <response code="500">Failed to queue email</response>
    // [HttpPost("test-queue")]
    // [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    // [ProducesResponseType(typeof(ErrorResponse), 400)]
    // [ProducesResponseType(typeof(ErrorResponse), 500)]
    // public async Task<IActionResult> TestQueueEmail(
    //     [FromBody] string to,
    //     [FromBody] string htmlContent,
    //     [FromQuery] string? subject = null)
    // {
    //     if (string.IsNullOrWhiteSpace(to))
    //     {
    //         return BadRequest(ApiResponse<object>.ErrorResponse("Email address is required"));
    //     }

    //     try
    //     {
    //         var _emailSubject = subject ?? "Queued Email from UCode";

    //         var emailMessage = new EmailQueueMessage
    //         {
    //             To = to,
    //             Subject = _emailSubject,
    //             HtmlContent = htmlContent ?? string.Empty
    //         };

    //         await _emailService.EnqueueEmailAsync(emailMessage);

    //         _logger.LogInformation("Email queued successfully for {Email}", to);

    //         return Ok(ApiResponse<object?>.SuccessResponse(
    //             null,
    //             $"Email queued successfully for {to}. It will be sent asynchronously."
    //         ));
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Failed to queue email to {Email}", to);
    //         return StatusCode(500, ApiResponse<object>.ErrorResponse(
    //             $"Failed to queue email: {ex.Message}"
    //         ));
    //     }
    // }

    // /// <summary>
    // /// Queuing multiple emails (async processing via RabbitMQ)
    // /// </summary>
    // /// <param name="emails">List of email addresses to send to</param>
    // /// <param name="htmlContent">HTML content of the email (optional)</param>
    // /// <param name="subject">Email subject (optional)</param>
    // /// <returns>Success message</returns>
    // /// <response code="200">Emails queued successfully</response>
    // /// <response code="400">Invalid request</response>
    // /// <response code="500">Failed to queue emails</response>
    // [HttpPost("queue-batch")]
    // [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    // [ProducesResponseType(typeof(ErrorResponse), 400)]
    // [ProducesResponseType(typeof(ErrorResponse), 500)]
    // public async Task<IActionResult> QueueBatchEmail(
    //     [FromBody] List<string> emails,
    //     [FromBody] string htmlContent,
    //     [FromQuery] string? subject = null)
    // {
    //     if (emails == null || emails.Count == 0)
    //     {
    //         return BadRequest(ApiResponse<object>.ErrorResponse("At least one email address is required"));
    //     }

    //     try
    //     {
    //         var _emailSubject = subject ?? "Queued Batch Email from UCode";
    //         var _htmlContent = htmlContent ?? string.Empty;

    //         var emailMessages = emails.Select(email => new EmailQueueMessage
    //         {
    //             To = email,
    //             Subject = _emailSubject,
    //             HtmlContent = _htmlContent
    //         }).ToList();

    //         await _emailService.EnqueueEmailsAsync(emailMessages);

    //         _logger.LogInformation("Batch emails queued successfully for {Count} recipients", emails.Count);

    //         return Ok(ApiResponse<object?>.SuccessResponse(
    //             null,
    //             $"Batch emails queued successfully for {emails.Count} recipients. They will be sent asynchronously."
    //         ));
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Failed to queue batch emails");
    //         return StatusCode(500, ApiResponse<object>.ErrorResponse(
    //             $"Failed to queue batch emails: {ex.Message}"
    //         ));
    //     }
    // }

    /// <summary>
    /// Tạo tài khoản cho sinh viên
    /// </summary>
    /// <param name="request">Request chứa danh sách tên, email và mật khẩu</param>
    /// <returns>Success message</returns>
    /// <response code="200">Account created successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Failed to create account</response>
    [HttpPost("queue-batch-create-accounts")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> QueueBatchCreateAccount(
        [FromBody] BatchCreateAccountRequest request)
    {
        if (request == null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Request body is required"));
        }

        if (request.Emails == null || request.Emails.Count == 0 ||
            request.FullNames == null || request.FullNames.Count == 0)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("At least one email address and full name is required"));
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Password is required"));
        }

        if (request.Emails.Count != request.FullNames.Count)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Number of emails must match number of full names"));
        }

        try
        {
            var emailMessages = new List<EmailQueueMessage>();
            for (int i = 0; i < request.Emails.Count && i < request.FullNames.Count; i++)
            {
                var emailMessage = new EmailQueueMessage
                {
                    To = request.Emails[i],
                    Subject = "Tạo tài khoản cho sinh viên",
                    HtmlContent = EmailTemplates.NewAccountWithTempPassword(
                        request.FullNames[i],
                        request.Emails[i],
                        request.Password,
                        "student")
                };
                emailMessages.Add(emailMessage);
            }

            await _emailService.EnqueueEmailsAsync(emailMessages);
            return Ok(ApiResponse<object?>.SuccessResponse(
                null,
                $"Account created successfully for {request.Emails.Count} recipients"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create account for {Count} emails", request.Emails?.Count ?? 0);
            return StatusCode(500, ApiResponse<object>.ErrorResponse($"Failed to create account: {ex.Message}"));
        }
    }

    /// <summary>
    /// Thêm nhiều sinh viên vào lớp học
    /// </summary>
    /// <param name="request">Request chứa danh sách email</param>
    /// <returns>Success message</returns>
    /// <response code="200">Students added to class successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Failed to add students to class</response>
    [HttpPost("queue-batch-add-students-to-class")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> QueueBatchAddStudentsToClass(
        [FromBody] BatchAddStudentsToClassRequest request)
    {
        if (request == null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Request body is required"));
        }

        if (request.Emails == null || request.Emails.Count == 0)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("At least one student id is required"));
        }

        if (string.IsNullOrWhiteSpace(request.ClassName))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Class name is required"));
        }

        try
        {
            var emailMessage = new EmailQueueMessage() {
                To = request.Emails.First(),
                Subject = "Thêm sinh viên vào lớp học",
                Bcc = request.Emails.Skip(1).ToList(),
                HtmlContent = EmailTemplates.NewClass(
                    request.ClassName,
                    request.TeacherName,
                    request.StartDate)
            };

            await _emailService.EnqueueEmailAsync(emailMessage);
            return Ok(ApiResponse<object?>.SuccessResponse(
                null,
                $"Students added to class successfully for {emailMessage.Subject} recipients"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add students to class for {Count} students", request.Emails?.Count ?? 0);
            return StatusCode(500, ApiResponse<object>.ErrorResponse($"Failed to add students to class: {ex.Message}"));
        }
    }
}