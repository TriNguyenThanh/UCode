using RabbitMQ.Client;
using SubmissionService.Application.Interfaces;
using System.Text;
using System.Text.Json;

namespace SubmissionService.Infrastructure.Microservices;

public class RabbitMqService : IDisposable, IRabbitMqService
{
    private readonly IRabbitMqConnectionProvider _provider;
    private IConnection _connection = null!;
    private IChannel _channel = null!;
    public RabbitMqService(IRabbitMqConnectionProvider provider)
    {
        _provider = provider;
    }

    private async Task CreateChanel()
    {
        _connection = await _provider.GetConnection();
        _channel = await _connection.CreateChannelAsync();
    }

    public async Task DeclareQueueAsync(string queueName)
    {
        await CreateChanel();
        await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
    }

    public async Task PublishMessageAsync<T>(T message, string queueName, bool madatory = false, BasicProperties properties = null!)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);
        await _channel.BasicPublishAsync(exchange: "", routingKey: queueName, mandatory: madatory, basicProperties: properties ?? new BasicProperties(), body: body);
    }

    void IDisposable.Dispose()
    {
        _channel?.CloseAsync();
        _channel?.Dispose();
    }
}