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
/// Consumer xử lý UserDeleted events từ user-service
/// </summary>
public class UserDeletedConsumer : BackgroundService
{
    private readonly IRabbitMqConnectionProvider _connectionProvider;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<UserDeletedConsumer> _logger;
    private const string QUEUE_NAME = "user_service.user_deleted";
    
    private IConnection? _connection;
    private IChannel? _channel;

    public UserDeletedConsumer(
        IRabbitMqConnectionProvider provider,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<UserDeletedConsumer> logger)
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
                    var @event = JsonSerializer.Deserialize<UserDeletedEvent>(message);

                    if (@event != null)
                    {
                        _logger.LogInformation("📩 Received UserDeleted event: UserId={UserId}",
                            @event.UserId);

                        await ProcessEventAsync(@event);

                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        
                        _logger.LogInformation("✅ Successfully processed UserDeleted event for UserId={UserId}",
                            @event.UserId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error processing UserDeleted event");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true); // Requeue on error
                }
            };

            await _channel.BasicConsumeAsync(QUEUE_NAME, false, consumer, stoppingToken);
            _logger.LogInformation("🚀 UserDeletedConsumer started listening on queue: {Queue}", QUEUE_NAME);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Critical error in UserDeletedConsumer");
        }
    }

    private async Task ProcessEventAsync(UserDeletedEvent @event)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var assignmentService = scope.ServiceProvider.GetRequiredService<IAssignmentService>();

        // Use the existing delete method in assignment service
        var success = await assignmentService.DeleteAssignmentUserByUserIdAsync(@event.UserId);

        if (success)
        {
            _logger.LogInformation("✅ Successfully deleted all assignment data for UserId={UserId}", @event.UserId);
        }
        else
        {
            _logger.LogWarning("⚠️ No assignment data found for UserId={UserId}", @event.UserId);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("UserDeletedConsumer stopping...");
        
        if (_channel != null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }

        await base.StopAsync(cancellationToken);
    }

    // Event model matching user-service
    private class UserDeletedEvent
    {
        public Guid UserId { get; set; }
        public DateTime OccurredAt { get; set; }
    }
}
