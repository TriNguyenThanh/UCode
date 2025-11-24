using Microsoft.AspNetCore.Mvc;
using AssignmentService.Application.Interfaces.Services;
using AssignmentService.Application.DTOs.Common;
using AssignmentService.Application.DTOs.Requests;

namespace AssignmentService.Api.Controllers;

/// <summary>
/// Controller for testing email service
/// </summary>
[ApiController]
[Route("api/v1/test/email")]
public class EmailTestController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailTestController> _logger;

    public EmailTestController(IEmailService emailService, ILogger<EmailTestController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Test sending an email
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject (optional)</param>
    /// <returns>Success message</returns>
    /// <response code="200">Email sent successfully</response>
    /// <response code="400">Invalid email address</response>
    /// <response code="500">Failed to send email</response>
    [HttpPost("test")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> TestSendEmail(
        [FromQuery] string to,
        [FromQuery] string? subject = null)
    {
        if (string.IsNullOrWhiteSpace(to))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Email address is required"));
        }

        try
        {
            var emailSubject = subject ?? "Test Email from UCode";
            var htmlContent = $@"
                <html>
                    <body>
                        <h2>Test Email from UCode</h2>
                        <p>This is a test email sent from the Assignment Service.</p>
                        <p>If you received this email, the email service is working correctly!</p>
                        <hr>
                        <p style='color: #666; font-size: 12px;'>Sent at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
                    </body>
                </html>";

            await _emailService.SendAsync(to, emailSubject, htmlContent);
            
            _logger.LogInformation("Test email sent successfully to {Email}", to);
            
            return Ok(ApiResponse<object?>.SuccessResponse(
                null, 
                $"Test email sent successfully to {to}"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send test email to {Email}", to);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                $"Failed to send email: {ex.Message}"
            ));
        }
    }

    /// <summary>
    /// Test sending multiple emails in parallel (batch)
    /// </summary>
    /// <param name="emails">List of email addresses to send to</param>
    /// <param name="subject">Email subject (optional)</param>
    /// <param name="maxConcurrency">Maximum number of concurrent emails (default: 10)</param>
    /// <returns>Success message</returns>
    /// <response code="200">Emails sent successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Failed to send emails</response>
    [HttpPost("test-batch")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> TestSendBatchEmail(
        [FromBody] List<string> emails,
        [FromQuery] string? subject = null,
        [FromQuery] int maxConcurrency = 10)
    {
        if (emails == null || emails.Count == 0)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("At least one email address is required"));
        }

        try
        {
            var emailSubject = subject ?? "Batch Test Email from UCode";
            var htmlContent = $@"
                <html>
                    <body>
                        <h2>Batch Test Email from UCode</h2>
                        <p>This is a batch test email sent from the Assignment Service.</p>
                        <p>Sent at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
                    </body>
                </html>";

            var emailMessages = emails.Select(email => new EmailQueueMessage
            {
                To = email,
                Subject = emailSubject,
                HtmlContent = htmlContent
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
        [FromQuery] string to,
        [FromQuery] string? subject = null)
    {
        if (string.IsNullOrWhiteSpace(to))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Email address is required"));
        }

        try
        {
            var emailSubject = subject ?? "Queued Test Email from UCode";
            var htmlContent = $@"
                <html>
                    <body>
                        <h2>Queued Test Email from UCode</h2>
                        <p>This email was queued and will be sent asynchronously via RabbitMQ.</p>
                        <p>Sent at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
                    </body>
                </html>";

            var emailMessage = new EmailQueueMessage
            {
                To = to,
                Subject = emailSubject,
                HtmlContent = htmlContent
            };

            await _emailService.EnqueueEmailAsync(emailMessage);
            
            _logger.LogInformation("Test email queued successfully for {Email}", to);
            
            return Ok(ApiResponse<object?>.SuccessResponse(
                null, 
                $"Test email queued successfully for {to}. It will be sent asynchronously."
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue test email to {Email}", to);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                $"Failed to queue email: {ex.Message}"
            ));
        }
    }

    /// <summary>
    /// Test queuing multiple emails (async processing via RabbitMQ)
    /// </summary>
    /// <param name="emails">List of email addresses to send to</param>
    /// <param name="subject">Email subject (optional)</param>
    /// <returns>Success message</returns>
    /// <response code="200">Emails queued successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Failed to queue emails</response>
    [HttpPost("test-queue-batch")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> TestQueueBatchEmail(
        [FromBody] List<string> emails,
        [FromQuery] string? subject = null)
    {
        if (emails == null || emails.Count == 0)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("At least one email address is required"));
        }

        try
        {
            var emailSubject = subject ?? "Queued Batch Test Email from UCode";
            var htmlContent = $@"
                <html>
                    <body>
                        <h2>Queued Batch Test Email from UCode</h2>
                        <p>This email was queued and will be sent asynchronously via RabbitMQ.</p>
                        <p>Sent at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
                    </body>
                </html>";

            var emailMessages = emails.Select(email => new EmailQueueMessage
            {
                To = email,
                Subject = emailSubject,
                HtmlContent = htmlContent
            }).ToList();

            await _emailService.EnqueueEmailsAsync(emailMessages);
            
            _logger.LogInformation("Batch test emails queued successfully for {Count} recipients", emails.Count);
            
            return Ok(ApiResponse<object?>.SuccessResponse(
                null, 
                $"Batch test emails queued successfully for {emails.Count} recipients. They will be sent asynchronously."
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue batch test emails");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                $"Failed to queue batch emails: {ex.Message}"
            ));
        }
    }
}