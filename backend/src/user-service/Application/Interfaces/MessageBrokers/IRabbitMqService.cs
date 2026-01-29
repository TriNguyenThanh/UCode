using RabbitMQ.Client;

namespace UserService.Application.Interfaces.MessageBrokers;

public interface IRabbitMqService
{
    Task DeclareQueueAsync(string queueName);
    Task PublishMessageAsync<T>(T message, string queueName, bool mandatory = false, BasicProperties? properties = null);
}
