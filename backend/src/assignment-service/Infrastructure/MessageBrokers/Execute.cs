using System.Net.Sockets;
using System.Threading.Tasks;
using AssignmentService.Application.DTOs.Common;
using AssignmentService.Application.Interfaces.MessageBrokers;
using AssignmentService.Application.Interfaces.Repositories;
using AssignmentService.Application.Interfaces.Services;
using AssignmentService.Domain.Entities;
using AssignmentService.Domain.Enums;

namespace AssignmentService.Infrastructure.MessageBrokers;

public class ExecuteService : IExecuteService
{
    private readonly ISubmissionRepository _repo;
    private readonly IRabbitMqService _rabbitMqService;
    private readonly IDatasetService _datasetService;
    private readonly ILanguageService _languageService;

    public ExecuteService(ISubmissionRepository repo, IRabbitMqService rabbitMqService, IDatasetService datasetService, ILanguageService languageService)
    {
        _repo = repo;
        _rabbitMqService = rabbitMqService;
        _datasetService = datasetService;
        _languageService = languageService;
    }
    public async Task ExecuteCode(Submission submission)
    {
        // lay du lieu data set tu Problem Service
        var dataset = await _datasetService.GetDatasetByIdAsync(submission.DatasetId);
        var language = await _languageService.GetLanguageByIdAsync(submission.LanguageId);
        var testcases = dataset?.TestCases;

        RabbitMqMessage message = null!;
        try
        {
            message = new RabbitMqMessage
            {
                SubmissionId = submission.SubmissionId.ToString(),
                Code = submission.SourceCode,
                Language = language!.Code,
                TimeLimit = Convert.ToInt32(language!.DefaultTimeFactor * 1000), // chuyen sang ms
                MemoryLimit = language!.DefaultMemoryKb,
                Testcases = testcases!.Select(tc => new TestCaseDto
                {
                    TestCaseId = tc.TestCaseId,
                    InputRef = tc.InputRef,
                    OutputRef = tc.OutputRef,
                    IndexNo = tc.IndexNo
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[x] Error preparing execution message: {ex.Message}");
            return;
        }

        // gui du lieu qua RabbitMQ server
        await _rabbitMqService.DeclareQueueAsync("submission_queue");
        await _rabbitMqService.PublishMessageAsync(
            queueName: "submission_queue",
            message: message,
            properties: new RabbitMQ.Client.BasicProperties
            {
                ReplyTo = "result_queue"
            });

        submission.PassedTestcase = 0;
        submission.TotalTestcase = testcases!.Count;
        submission.Status = SubmissionStatus.Running;
        await _repo.UpdateSubmission(submission);
        Console.WriteLine($"[x] Sent submission {submission.SubmissionId} to execution service");
        // await _repo.Detach(submission);
    }
}