using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AssignmentService.Application.Interfaces.MessageBrokers;
using AssignmentService.Application.Interfaces.Services;

namespace AssignmentService.Infrastructure.BackgroundServices;

/// <summary>
/// Consumer xử lý StudentRemovedFromClass events từ user-service
/// </summary>
public class StudentRemovedFromClassConsumer : BackgroundService
{
    private readonly IRabbitMqConnectionProvider _connectionProvider;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<StudentRemovedFromClassConsumer> _logger;
    private const string QUEUE_NAME = "user_service.student_removed_from_class";
    
    private IConnection? _connection;
    private IChannel? _channel;

    public StudentRemovedFromClassConsumer(
        IRabbitMqConnectionProvider provider,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<StudentRemovedFromClassConsumer> logger)
    {
        _connectionProvider = provider;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(2000, stoppingToken); // Wait for services to start

        try
        {
            _connection = await _connectionProvider.GetConnection();
            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(
                queue: QUEUE_NAME,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var @event = JsonSerializer.Deserialize<StudentRemovedFromClassEvent>(message);

                    if (@event != null)
                    {
                        _logger.LogInformation("📩 Received StudentRemovedFromClass event: UserId={UserId}, ClassId={ClassId}",
                            @event.UserId, @event.ClassId);

                        await ProcessEventAsync(@event);

                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        
                        _logger.LogInformation("✅ Successfully processed StudentRemovedFromClass event for UserId={UserId}, ClassId={ClassId}",
                            @event.UserId, @event.ClassId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error processing StudentRemovedFromClass event");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true); // Requeue on error
                }
            };

            await _channel.BasicConsumeAsync(QUEUE_NAME, false, consumer, stoppingToken);
            _logger.LogInformation("🚀 StudentRemovedFromClassConsumer started listening on queue: {Queue}", QUEUE_NAME);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Critical error in StudentRemovedFromClassConsumer");
        }
    }

    private async Task ProcessEventAsync(StudentRemovedFromClassEvent @event)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var assignmentService = scope.ServiceProvider.GetRequiredService<IAssignmentService>();

        // Delete assignment users only for the specific class
        var success = await assignmentService.DeleteAssignmentUserByUserIdAndClassIdAsync(@event.UserId, @event.ClassId);

        if (success)
        {
            _logger.LogInformation("✅ Successfully deleted assignment data for UserId={UserId} in ClassId={ClassId}", 
                @event.UserId, @event.ClassId);
        }
        else
        {
            _logger.LogWarning("⚠️ No assignment data found for UserId={UserId} in ClassId={ClassId}", 
                @event.UserId, @event.ClassId);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("StudentRemovedFromClassConsumer stopping...");
        
        if (_channel != null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }

        await base.StopAsync(cancellationToken);
    }

    // Event model matching user-service
    private class StudentRemovedFromClassEvent
    {
        public Guid UserId { get; set; }
        public Guid ClassId { get; set; }
        public DateTime OccurredAt { get; set; }
    }
}
