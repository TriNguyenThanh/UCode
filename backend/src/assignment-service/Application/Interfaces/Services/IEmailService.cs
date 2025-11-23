using AssignmentService.Application.DTOs.Requests;

namespace AssignmentService.Application.Interfaces.Services;
public interface IEmailService
{
    /// <summary>
    /// Gửi một email đơn lẻ
    /// </summary>
    Task SendAsync(string to, string subject, string htmlContent);
    
    /// <summary>
    /// Gửi nhiều email song song (parallel) - Phù hợp cho < 100 emails
    /// </summary>
    Task SendBatchAsync(List<EmailQueueMessage> emails, int maxConcurrency = 10);
    
    /// <summary>
    /// Đưa email vào hàng đợi RabbitMQ để xử lý bất đồng bộ - Phù hợp cho nhiều emails hoặc không muốn block request
    /// </summary>
    Task EnqueueEmailAsync(EmailQueueMessage email);
    
    /// <summary>
    /// Đưa nhiều email vào hàng đợi RabbitMQ
    /// </summary>
    Task EnqueueEmailsAsync(List<EmailQueueMessage> emails);
}
