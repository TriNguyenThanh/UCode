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

    /// <summary>
    /// Sending multiple emails in parallel (batch)
    /// </summary>
    /// <param name="emails">List of email addresses to send to</param>
    /// <param name="subject">Email subject (optional)</param>
    /// <param name="htmlContent">HTML content of the email (optional)</param>
    /// <param name="maxConcurrency">Maximum number of concurrent emails (default: 10)</param>
    /// <returns>Success message</returns>
    /// <response code="200">Emails sent successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Failed to send emails</response>
    [HttpPost("send-batch")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> SendBatchEmail(
        [FromBody] List<string> emails,
        [FromBody] string htmlContent,
        [FromQuery] string? subject = null,
        [FromQuery] int maxConcurrency = 10)
    {
        if (emails == null || emails.Count == 0)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("At least one email address is required"));
        }

        try
        {
            var _emailSubject = subject ?? "Batch Email from UCode";

            var emailMessages = emails.Select(email => new EmailQueueMessage
            {
                To = email,
                Subject = _emailSubject,
                HtmlContent = htmlContent ?? string.Empty
            }).ToList();

            await _emailService.SendBatchAsync(emailMessages, maxConcurrency);
            
            _logger.LogInformation("Batch test emails sent successfully to {Count} recipients", emails.Count);
            
            return Ok(ApiResponse<object?>.SuccessResponse(
                null, 
                $"Batch test emails sent successfully to {emails.Count} recipients"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send batch test emails");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                $"Failed to send batch emails: {ex.Message}"
            ));
        }
    }

    /// <summary>
    /// Test queuing an email (async processing via RabbitMQ)
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="htmlContent">HTML content of the email (optional)</param>
    /// <param name="subject">Email subject (optional)</param>
    /// <returns>Success message</returns>
    /// <response code="200">Email queued successfully</response>
    /// <response code="400">Invalid email address</response>
    /// <response code="500">Failed to queue email</response>
    [HttpPost("test-queue")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> TestQueueEmail(
        [FromBody] string to,
        [FromBody] string htmlContent,
        [FromQuery] string? subject = null)
    {
        if (string.IsNullOrWhiteSpace(to))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Email address is required"));
        }

        try
        {
            var _emailSubject = subject ?? "Queued Email from UCode";

            var emailMessage = new EmailQueueMessage
            {
                To = to,
                Subject = _emailSubject,
                HtmlContent = htmlContent ?? string.Empty
            };

            await _emailService.EnqueueEmailAsync(emailMessage);
            
            _logger.LogInformation("Email queued successfully for {Email}", to);
            
            return Ok(ApiResponse<object?>.SuccessResponse(
                null, 
                $"Email queued successfully for {to}. It will be sent asynchronously."
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue email to {Email}", to);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                $"Failed to queue email: {ex.Message}"
            ));
        }
    }

    /// <summary>
    /// Queuing multiple emails (async processing via RabbitMQ)
    /// </summary>
    /// <param name="emails">List of email addresses to send to</param>
    /// <param name="htmlContent">HTML content of the email (optional)</param>
    /// <param name="subject">Email subject (optional)</param>
    /// <returns>Success message</returns>
    /// <response code="200">Emails queued successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Failed to queue emails</response>
    [HttpPost("queue-batch")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> QueueBatchEmail(
        [FromBody] List<string> emails,
        [FromBody] string htmlContent,
        [FromQuery] string? subject = null)
    {
        if (emails == null || emails.Count == 0)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("At least one email address is required"));
        }

        try
        {
            var _emailSubject = subject ?? "Queued Batch Email from UCode";
            var _htmlContent = htmlContent ?? string.Empty;

            var emailMessages = emails.Select(email => new EmailQueueMessage
            {
                To = email,
                Subject = _emailSubject,
                HtmlContent = _htmlContent
            }).ToList();

            await _emailService.EnqueueEmailsAsync(emailMessages);
            
            _logger.LogInformation("Batch emails queued successfully for {Count} recipients", emails.Count);
            
            return Ok(ApiResponse<object?>.SuccessResponse(
                null, 
                $"Batch emails queued successfully for {emails.Count} recipients. They will be sent asynchronously."
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue batch emails");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                $"Failed to queue batch emails: {ex.Message}"
            ));
        }
    }
}