using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using AssignmentService.Domain.Enums;
using AssignmentService.Domain.Entities;
using AssignmentService.Application.Interfaces.MessageBrokers;
using AssignmentService.Application.Interfaces.Repositories;
using AssignmentService.Application.DTOs.Responses;
using AssignmentService.Application.Interfaces.Services;

namespace AssignmentService.Infrastructure.BackgroundServices;

public class ResultConsumer : BackgroundService
{
    private readonly IRabbitMqConnectionProvider _connectionProvider;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly int _maxDegreeOfParallelism = 10; // Set max threads/tasks

    public ResultConsumer(IRabbitMqConnectionProvider provider, IServiceScopeFactory serviceScopeFactory)
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
            queue: "result_queue",
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
                    Console.WriteLine($"❌ Error processing message: {ex.Message}");
                    // Nack để không requeue (tránh infinite loop), có thể config DLQ
                    await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
                }
                finally
                {
                    semaphore.Release();
                }
            }, stoppingToken);
        };

        // Start consuming messages
        var consumerTag = await channel.BasicConsumeAsync("result_queue", autoAck: false, consumer: consumer);

        // giữ service sống cho tới khi cancel
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException) { }

        // khi stoppingToken cancelled -> hủy consumer, đóng channel/connection
        await channel.BasicCancelAsync(consumerTag);
        await channel.CloseAsync();
        await connection.CloseAsync();
    }

    private async Task ProcessMessage(BasicDeliverEventArgs ea, CancellationToken stoppingToken)
    {
        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
        var options = new JsonSerializerOptions
        {
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
            PropertyNameCaseInsensitive = true
        };

        var results_message = JsonSerializer.Deserialize<ResultMessageResponse>(json, options);
        Console.WriteLine($"[x] Received a result message from result_queue with submissionId {results_message?.SubmissionId}");

        using (var scope = _serviceScopeFactory.CreateAsyncScope())
        {
            var submissionService = scope.ServiceProvider.GetRequiredService<ISubmissionService>();

            if (results_message != null)
            {

                int testcasePassed = 0;
                foreach (var result in results_message.CompileResult)
                {
                    if (result == '0')
                    {
                        testcasePassed++;
                    }
                }
                var submission = await submissionService.GetSubmission(results_message.SubmissionId);
                submission.Score = await submissionService.Getscore(submission);
                submission.PassedTestcase = testcasePassed;
                submission.TotalTime = results_message.TotalTime;
                submission.TotalMemory = results_message.TotalMemory;
                submission.Status = (submission.PassedTestcase == submission.TotalTestcase) ? SubmissionStatus.Passed : SubmissionStatus.Failed;
                submission.ErrorCode = results_message.ErrorCode;
                submission.ErrorMessage = results_message.ErrorMessage;
                submission.CompareResult = results_message.CompileResult;

                if (await submissionService.UpdateSubmission(submission))
                {
                    Console.WriteLine($"[x] Updated submission {submission.SubmissionId} with status {submission.Status}");
                }

                // AttemptCount = await submissionRepo.GetNumberOfSubmissionPerProblemId(submission.AssignmentId, submission.ProblemId, submission.UserId);
            }
        }
        
        // websocket notify user
    }
}
