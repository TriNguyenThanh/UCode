using RabbitMQ.Client;

namespace UserService.Application.Interfaces.MessageBrokers;

public interface IRabbitMqConnectionProvider
{
    Task<IConnection> GetConnection();
}
