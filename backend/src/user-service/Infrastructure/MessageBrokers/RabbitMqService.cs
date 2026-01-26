using RabbitMQ.Client;
using UserService.Application.Interfaces.MessageBrokers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace UserService.Infrastructure.MessageBrokers;

public class RabbitMqService : IDisposable, IRabbitMqService
{
    private readonly IRabbitMqConnectionProvider _provider;
    private readonly ILogger<RabbitMqService> _logger;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly SemaphoreSlim _channelLock = new(1, 1);
    private bool _disposed = false;

    public RabbitMqService(
        IRabbitMqConnectionProvider provider,
        ILogger<RabbitMqService> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    private async Task EnsureChannelAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_channel != null && _channel.IsOpen)
        {
            return;
        }

        await _channelLock.WaitAsync();
        try
        {
            if (_channel != null && _channel.IsOpen)
            {
                return;
            }

            if (_channel != null)
            {
                try
                {
                    await _channel.CloseAsync();
                    _channel.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error closing existing channel");
                }
                _channel = null;
            }

            _connection = await _provider.GetConnection();
            _channel = await _connection.CreateChannelAsync();
            
            _logger.LogDebug("RabbitMQ channel created successfully");
        }
        finally
        {
            _channelLock.Release();
        }
    }

    public async Task DeclareQueueAsync(string queueName)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        await EnsureChannelAsync();
        
        await _channel!.QueueDeclareAsync(
            queue: queueName, 
            durable: true, 
            exclusive: false, 
            autoDelete: false, 
            arguments: null);
            
        _logger.LogDebug("Queue '{QueueName}' declared", queueName);
    }

    public async Task PublishMessageAsync<T>(T message, string queueName, bool mandatory = false, BasicProperties? properties = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        await EnsureChannelAsync();
        
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);
        
        await _channel!.BasicPublishAsync(
            exchange: "", 
            routingKey: queueName, 
            mandatory: mandatory, 
            basicProperties: properties ?? new BasicProperties(), 
            body: body);
            
        _logger.LogDebug("Message published to queue '{QueueName}'", queueName);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_channelLock.Wait(TimeSpan.FromSeconds(5)))
        {
            try
            {
                if (_channel != null)
                {
                    try
                    {
                        _channel.CloseAsync().GetAwaiter().GetResult();
                        _channel.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error disposing RabbitMQ channel");
                    }
                    _channel = null;
                }
            }
            finally
            {
                _channelLock.Release();
            }
        }
        
        _channelLock.Dispose();
        _disposed = true;
        
        GC.SuppressFinalize(this);
    }
}
