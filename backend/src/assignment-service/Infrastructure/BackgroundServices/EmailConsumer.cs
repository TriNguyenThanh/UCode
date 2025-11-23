using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AssignmentService.Application.Interfaces.MessageBrokers;
using AssignmentService.Application.Interfaces.Services;
using AssignmentService.Application.DTOs.Requests;

namespace AssignmentService.Infrastructure.BackgroundServices;

/// <summary>
/// Background service để consume và gửi email từ RabbitMQ queue
/// </summary>
public class EmailConsumer : BackgroundService
{
    private readonly IRabbitMqConnectionProvider _connectionProvider;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<EmailConsumer> _logger;
    private readonly int _maxDegreeOfParallelism = 10; // Số lượng email gửi đồng thời

    public EmailConsumer(
        IRabbitMqConnectionProvider provider, 
        IServiceScopeFactory serviceScopeFactory,
        ILogger<EmailConsumer> logger)
    {
        _connectionProvider = provider;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await _connectionProvider.GetConnection();
        var channel = await connection.CreateChannelAsync();

        // Declare the queue
        await channel.QueueDeclareAsync(
            queue: "email_queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        // Set QoS để không nhận quá nhiều message cùng lúc
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: (ushort)_maxDegreeOfParallelism, global: false);

        // Create a consumer
        var consumer = new AsyncEventingBasicConsumer(channel);
        var semaphore = new SemaphoreSlim(_maxDegreeOfParallelism, _maxDegreeOfParallelism);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            await semaphore.WaitAsync(stoppingToken);
            _ = Task.Run(async () =>
            {
                try
                {
                    await ProcessEmailMessage(ea, stoppingToken);
                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error processing email message");
                    // Nack để không requeue (tránh infinite loop), có thể config DLQ sau
                    await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
                }
                finally
                {
                    semaphore.Release();
                }
            }, stoppingToken);
        };

        // Start consuming messages
        var consumerTag = await channel.BasicConsumeAsync("email_queue", autoAck: false, consumer: consumer);
        _logger.LogInformation("📧 EmailConsumer started and listening on email_queue");

        // Giữ service sống cho tới khi cancel
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException) { }

        // Khi stoppingToken cancelled -> hủy consumer, đóng channel/connection
        await channel.BasicCancelAsync(consumerTag);
        await channel.CloseAsync();
        await connection.CloseAsync();
        _logger.LogInformation("📧 EmailConsumer stopped");
    }

    private async Task ProcessEmailMessage(BasicDeliverEventArgs ea, CancellationToken stoppingToken)
    {
        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var emailMessage = JsonSerializer.Deserialize<EmailQueueMessage>(json, options);
        
        if (emailMessage == null)
        {
            _logger.LogWarning("Received null email message");
            return;
        }

        _logger.LogInformation("📧 Processing email to {Email}", emailMessage.To);

        using (var scope = _serviceScopeFactory.CreateAsyncScope())
        {
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            try
            {
                await emailService.SendAsync(emailMessage.To, emailMessage.Subject, emailMessage.HtmlContent);
                _logger.LogInformation("✅ Email sent successfully to {Email}", emailMessage.To);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send email to {Email}", emailMessage.To);
                throw; // Re-throw để nack message
            }
        }
    }
}

