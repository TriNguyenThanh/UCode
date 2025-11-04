using AssignmentService.Application.DTOs.Common;
using AssignmentService.Application.Interfaces.MessageBrokers;
using AssignmentService.Application.Interfaces.Repositories;
using AssignmentService.Domain.Entities;
using AssignmentService.Domain.Enums;

namespace AssignmentService.Infrastructure.MessageBrokers;

public class ExecuteService : IExecuteService
{
    private readonly ISubmissionRepository _repo;
    private readonly IRabbitMqService _rabbitMqService;

    public ExecuteService(ISubmissionRepository repo, IRabbitMqService rabbitMqService)
    {
        _repo = repo;
        _rabbitMqService = rabbitMqService;
    }
    public async Task ExecuteCode(Submission submission)
    {
        // lay du lieu data set tu Problem Service
        var dataset = GetDatasetDto(submission.DatasetId);
        var testcases = dataset?.TestCases ?? new List<TestCaseDto>();

        var message = new RabbitMqMessage
        {
            SubmissionId = submission.SubmissionId.ToString(),
            Code = submission.SourceCode,
            Language = submission.Language,
            TimeLimit = 2,
            MemoryLimit = 128,
            Testcases = testcases
        };

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
        submission.TotalTestcase = testcases.Count;
        submission.Status = SubmissionStatus.Running;
        await _repo.UpdateSubmission(submission);
        Console.WriteLine($"[x] Sent submission {submission.SubmissionId} to execution service");
        // await _repo.Detach(submission);
    }

    private DatasetDto GetDatasetDto(Guid datasetId)
    {
        var datasets = new List<DatasetDto>
        {
            new DatasetDto
            {
                DatasetId = Guid.Parse("56F4FA4A-3282-4D0E-AE4F-4D55EFBAE734"),
                ProblemId = Guid.Parse("CBE50FA4-36F6-421D-B02B-24261F4E3A0D"),
                Name = "Sample Tests",
                Kind = DatasetKind.SAMPLE,
                TestCases = new List<TestCaseDto>
                    {
                        new TestCaseDto
                        {
                            TestCaseId = Guid.NewGuid(),
                            IndexNo = 0,
                            InputRef = "1 2\n",
                            OutputRef = "3\n"
                        },
                        new TestCaseDto
                        {
                            TestCaseId = Guid.NewGuid(),
                            IndexNo = 1,
                            InputRef = "10 20\n",
                            OutputRef = "30\n"
                        }
                    }
            },
            new DatasetDto
            {
                DatasetId = Guid.Parse("9764A974-B833-4323-B632-B7EA1DD62BC2"),
                ProblemId = Guid.Parse("CBE50FA4-36F6-421D-B02B-24261F4E3A0D"),
                Name = "Private Tests",
                Kind = DatasetKind.PRIVATE,
                TestCases = new List<TestCaseDto>
                        {
                            new TestCaseDto
                            {
                                TestCaseId = Guid.NewGuid(),
                                IndexNo = 0,
                                InputRef = "1 2\n",
                                OutputRef = "3\n"
                            },
                            new TestCaseDto
                            {
                                TestCaseId = Guid.NewGuid(),
                                IndexNo = 1,
                                InputRef = "10 20\n",
                                OutputRef = "30\n"
                            },
                            new TestCaseDto
                            {
                                TestCaseId = Guid.NewGuid(),
                                IndexNo = 2,
                                InputRef = "100 200\n",
                                OutputRef = "300\n"
                            },
                            new TestCaseDto
                            {
                                TestCaseId = Guid.NewGuid(),
                                IndexNo = 3,
                                InputRef = "1000 2000\n",
                                OutputRef = "3000\n"
                            }
                        }
            },
        };

        var dataset = datasets.FirstOrDefault(d => d.DatasetId == datasetId);
        if (dataset == null)
        {
            Console.WriteLine($"[!] Dataset with ID {datasetId} not found.");
            return new DatasetDto()
            {
                DatasetId = datasetId,
                ProblemId = Guid.Empty,
                Name = "Unknown",
                Kind = DatasetKind.SAMPLE,
                TestCases = new List<TestCaseDto>()
            };
        }
        return dataset;
    }
}