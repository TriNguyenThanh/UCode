using RabbitMQ.Client;
using System.Threading.Tasks;

namespace AssignmentService.Application.Interfaces.MessageBrokers;

public interface IRabbitMqConnectionProvider
{
    Task<IConnection> GetConnection();
}