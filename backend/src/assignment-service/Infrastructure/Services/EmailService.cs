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
        _resend = resend ?? throw new ArgumentNullException(nameof(resend));
        _rabbitMqService = rabbitMqService ?? throw new ArgumentNullException(nameof(rabbitMqService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // ✅ Validate From address
        _from = config["Resend:From"];
        if (string.IsNullOrWhiteSpace(_from))
        {
            throw new InvalidOperationException(
                "Resend:From configuration is required. " +
                "For testing, use 'onboarding@resend.dev'. " +
                "For production, use your verified domain.");
        }
        
        _logger.LogInformation("✅ EmailService initialized with From: {From}", _from);
    }

    public async Task SendAsync(string to, string subject, string htmlContent)
    {
        // ✅ Validate inputs
        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("Recipient email cannot be empty", nameof(to));
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject cannot be empty", nameof(subject));
        if (string.IsNullOrWhiteSpace(htmlContent))
            throw new ArgumentException("HTML content cannot be empty", nameof(htmlContent));

        var message = new EmailMessage
        {
            From = _from,
            To = { to },
            Subject = subject,
            HtmlBody = htmlContent
        };

        try
        {
            var response = await _resend.EmailSendAsync(message);
            // _logger.LogInformation("✅ Email sent to {To}, ID: {MessageId}", to, response.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send email to {To}", to);
            throw;
        }
    }

    public async Task SendWithBccAsync(string to, List<string>? bcc, string subject, string htmlContent)
    {
        // ✅ Validate inputs
        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("Recipient email cannot be empty", nameof(to));
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject cannot be empty", nameof(subject));
        if (string.IsNullOrWhiteSpace(htmlContent))
            throw new ArgumentException("HTML content cannot be empty", nameof(htmlContent));

        _logger.LogInformation("📧 Preparing email to {To} with {BccCount} BCC recipients", to, bcc?.Count ?? 0);

        var message = new EmailMessage
        {
            From = _from,
            To = { to },
            Bcc = new EmailAddressList(),
            Subject = subject,
            HtmlBody = htmlContent
        };

        // ✅ Add BCC recipients
        if (bcc != null && bcc.Count > 0)
        {
            var validBcc = bcc.Where(email => !string.IsNullOrWhiteSpace(email)).ToList();
            _logger.LogInformation("Adding {Count} valid BCC recipients", validBcc.Count);
            
            foreach (var bccEmail in validBcc)
            {
                message.Bcc.Add(bccEmail);  // ✅ FIX: Add the EmailAddressItem
            }
        }

        try
        {
            var response = await _resend.EmailSendAsync(message);
            // _logger.LogInformation("✅ Email sent to {To} with {BccCount} BCC, ID: {MessageId}", 
                // to, bcc?.Count ?? 0, response.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send email to {To} with BCC", to);
            throw;
        }
    }

    public async Task SendBatchAsync(List<EmailQueueMessage> emails, int maxConcurrency = 10)
    {
        if (emails == null || emails.Count == 0)
        {
            _logger.LogWarning("SendBatchAsync called with empty email list");
            return;
        }

        var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        var tasks = emails.Select(async email =>
        {
            await semaphore.WaitAsync();
            try
            {
                await SendAsync(email.To, email.Subject, email.HtmlContent);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
        _logger.LogInformation("✅ Sent {Count} emails in batch", emails.Count);
    }

    public async Task EnqueueEmailAsync(EmailQueueMessage email)
    {
        // ✅ Validate email message
        if (email == null)
            throw new ArgumentNullException(nameof(email));
        if (string.IsNullOrWhiteSpace(email.To))
            throw new ArgumentException("Email recipient cannot be empty", nameof(email));

        try
        {
            await _rabbitMqService.DeclareQueueAsync("email_queue");
            await _rabbitMqService.PublishMessageAsync(email, "email_queue");
            _logger.LogInformation("📬 Email queued for {Email}", email.To);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to queue email for {Email}", email.To);
            throw;
        }
    }

    public async Task EnqueueEmailsAsync(List<EmailQueueMessage> emails)
    {
        if (emails == null || emails.Count == 0)
        {
            _logger.LogWarning("EnqueueEmailsAsync called with empty email list");
            return;
        }

        try
        {
            await _rabbitMqService.DeclareQueueAsync("email_queue");

            foreach (var email in emails)
            {
                await _rabbitMqService.PublishMessageAsync(email, "email_queue");
            }

            _logger.LogInformation("📬 Queued {Count} emails", emails.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to queue emails");
            throw;
        }
    }
}
