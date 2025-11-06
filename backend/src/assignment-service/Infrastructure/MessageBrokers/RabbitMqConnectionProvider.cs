using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using AssignmentService.Application.Interfaces.MessageBrokers;

namespace AssignmentService.Infrastructure.MessageBrokers;

public class RabbitMqConnectionProvider : IRabbitMqConnectionProvider, IDisposable
{
    private IConnection? _connection = null;
    private readonly IConfiguration _config;
    
    public RabbitMqConnectionProvider(IConfiguration configuration)
    {
        _config = configuration;
    }

    public async Task<IConnection> GetConnection()
    {
        try
        {
            if (_connection == null)
            {
                var _factory = new ConnectionFactory
                {
                    HostName = _config["RabbitMQ:Host"] ?? "rabbitmq",
                    Port = int.Parse(_config["RabbitMQ:Port"] ?? "5672"),
                    UserName = _config["RabbitMQ:Username"] ?? "guest",
                    Password = _config["RabbitMQ:Password"] ?? "guest",
                };
                _connection = await _factory.CreateConnectionAsync();
            }
            return _connection;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating RabbitMQ connection: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_connection != null)  // Fixed: null check
        {
            _connection.CloseAsync().GetAwaiter().GetResult();
            _connection.Dispose();
        }
    }
}