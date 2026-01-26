namespace AssignmentService.Infrastructure.BackgroundServices;

using AssignmentService.Application.Interfaces.MessageBrokers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using AssignmentService.Application.Interfaces.Services;
using System.Text.Json;
using AssignmentService.Application.DTOs.Requests;

public class CodeFormatterConsumer : BackgroundService
{
    private readonly IRabbitMqConnectionProvider _connectionProvider;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly int _maxDegreeOfParallelism = 5; // Set max threads/tasks

    public CodeFormatterConsumer(IRabbitMqConnectionProvider provider, IServiceScopeFactory serviceScopeFactory)
    {
        _connectionProvider = provider;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await _connectionProvider.GetConnection();
        var channel = await connection.CreateChannelAsync();

        // Declare the queue (make sure it exists)
        await channel.QueueDeclareAsync(
            queue: "code_formatter_queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        // Create a consumer and define the Received event handler
        var consumer = new AsyncEventingBasicConsumer(channel);
        var semaphore = new SemaphoreSlim(_maxDegreeOfParallelism);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            await semaphore.WaitAsync(stoppingToken);
            _ = Task.Run(async () =>
            {
                try
                {
                    await ProcessMessage(ea, stoppingToken);
                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                    // Nack để không requeue (tránh infinite loop), có thể config DLQ
                    await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
                }
                finally
                {
                    semaphore.Release();
                }
            }, stoppingToken);
        };

        await channel.BasicConsumeAsync(
            queue: "code_formatter_queue",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken
        );
    }

    private async Task ProcessMessage(BasicDeliverEventArgs ea, CancellationToken stoppingToken)
    {
        try
        {
            var message = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());
            var formatCodeMessage = JsonSerializer.Deserialize<FormatCodeMessage>(message);
            Console.WriteLine($"Received message for code formatting: {message}");
            if (formatCodeMessage == null)
            {
                throw new Exception("Invalid message format");
            }
            using var scope = _serviceScopeFactory.CreateScope();
            var codeFormatterService = scope.ServiceProvider.GetRequiredService<ICodeFormatterService>();
            await codeFormatterService.FormatCode(formatCodeMessage.SubmissionId);

            Console.WriteLine($"Finished processing code formatting for message: {message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in ProcessMessage: {ex.Message}");
            throw; // Rethrow để BasicNack được gọi
        }
    }
}