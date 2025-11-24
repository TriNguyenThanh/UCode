using Resend;
using AssignmentService.Application.Interfaces.Services;
using AssignmentService.Application.Interfaces.MessageBrokers;
using AssignmentService.Application.DTOs.Requests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AssignmentService.Infrastructure.Services;
public class EmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly IRabbitMqService _rabbitMqService;
    private readonly string _from;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IResend resend,
        IConfiguration config,
        IRabbitMqService rabbitMqService,
        ILogger<EmailService> logger)
    {
        _resend = resend;
        _rabbitMqService = rabbitMqService;
        _from = config["Resend:From"] ?? throw new InvalidOperationException("Resend:From configuration is required");
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlContent)
    {
        var message = new EmailMessage
        {
            From = _from,
            To = { to },
            Subject = subject,
            HtmlBody = htmlContent
        };

        await _resend.EmailSendAsync(message);
    }

    /// <summary>
    /// Gửi email với BCC (Blind Carbon Copy)
    /// </summary>
    public async Task SendWithBccAsync(string to, List<string>? bcc, string subject, string htmlContent)
    {
        var message = new EmailMessage
        {
            From = _from,
            To = { to },
            Subject = subject,
            HtmlBody = htmlContent
        };

        // Thêm BCC nếu có
        if (bcc != null && bcc.Count > 0)
        {
            foreach (var bccEmail in bcc.Where(email => !string.IsNullOrWhiteSpace(email)))
            {
                message.Bcc.Add(bccEmail);
            }
        }

        await _resend.EmailSendAsync(message);
        _logger.LogInformation("Email sent to {To} with {BccCount} BCC recipients", to, bcc?.Count ?? 0);
    }

    /// <summary>
    /// Gửi nhiều email song song với giới hạn concurrency
    /// </summary>
    public async Task SendBatchAsync(List<EmailQueueMessage> emails, int maxConcurrency = 10)
    {
        if (emails == null || emails.Count == 0)
            return;

        var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        var tasks = emails.Select(async email =>
        {
            await semaphore.WaitAsync();
            try
            {
                await SendAsync(email.To, email.Subject, email.HtmlContent);
                _logger.LogInformation("Sent email to {Email}", email.To);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", email.To);
                throw; // Re-throw để caller có thể handle
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Đưa email vào hàng đợi RabbitMQ
    /// </summary>
    public async Task EnqueueEmailAsync(EmailQueueMessage email)
    {
        try
        {
            await _rabbitMqService.DeclareQueueAsync("email_queue");
            await _rabbitMqService.PublishMessageAsync(email, "email_queue");
            _logger.LogInformation("Email queued for {Email}", email.To);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue email for {Email}", email.To);
            throw;
        }
    }

    /// <summary>
    /// Đưa nhiều email vào hàng đợi RabbitMQ
    /// </summary>
    public async Task EnqueueEmailsAsync(List<EmailQueueMessage> emails)
    {
        if (emails == null || emails.Count == 0)
            return;

        try
        {
            await _rabbitMqService.DeclareQueueAsync("email_queue");

            foreach (var email in emails)
            {
                await _rabbitMqService.PublishMessageAsync(email, "email_queue");
            }

            _logger.LogInformation("Queued {Count} emails", emails.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue emails");
            throw;
        }
    }
}
