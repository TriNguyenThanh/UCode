using RabbitMQ.Client;

namespace AssignmentService.Application.Interfaces.MessageBrokers;

public interface IRabbitMqService
{
    Task DeclareQueueAsync(string queueName);
    Task PublishMessageAsync<T>(T message, string queueName, bool madatory = false, BasicProperties properties = null);
}