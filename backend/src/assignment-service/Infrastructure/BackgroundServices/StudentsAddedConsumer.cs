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
/// Consumer xử lý StudentsAddedToClass events từ user-service
/// </summary>
public class StudentsAddedConsumer : BackgroundService
{
    private readonly IRabbitMqConnectionProvider _connectionProvider;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<StudentsAddedConsumer> _logger;
    private const string QUEUE_NAME = "user_service.students_added";
    
    private IConnection? _connection;
    private IChannel? _channel;

    public StudentsAddedConsumer(
        IRabbitMqConnectionProvider provider,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<StudentsAddedConsumer> logger)
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
                    var @event = JsonSerializer.Deserialize<StudentsAddedToClassEvent>(message);

                    if (@event != null)
                    {
                        _logger.LogInformation("📩 Received StudentsAdded event: ClassId={ClassId}, Students={Count}",
                            @event.ClassId, @event.StudentIds.Count);

                        await ProcessEventAsync(@event);

                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        
                        _logger.LogInformation("✅ Successfully processed StudentsAdded event for ClassId={ClassId}",
                            @event.ClassId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error processing StudentsAdded event");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true); // Requeue on error
                }
            };

            await _channel.BasicConsumeAsync(QUEUE_NAME, false, consumer, stoppingToken);
            _logger.LogInformation("🚀 StudentsAddedConsumer started listening on queue: {Queue}", QUEUE_NAME);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Critical error in StudentsAddedConsumer");
        }
    }

    private async Task ProcessEventAsync(StudentsAddedToClassEvent @event)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var assignmentService = scope.ServiceProvider.GetRequiredService<IAssignmentService>();

        // Use the existing sync method in assignment service
        var count = await assignmentService.SyncStudentsToClassAssignmentsAsync(@event.ClassId, @event.StudentIds);

        _logger.LogInformation("✅ Created {Count} AssignmentUser records for ClassId={ClassId}", 
            count, @event.ClassId);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("StudentsAddedConsumer stopping...");
        
        if (_channel != null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }

        await base.StopAsync(cancellationToken);
    }

    // Event model matching user-service
    private class StudentsAddedToClassEvent
    {
        public Guid ClassId { get; set; }
        public List<Guid> StudentIds { get; set; } = new();
        public DateTime OccurredAt { get; set; }
    }
}
