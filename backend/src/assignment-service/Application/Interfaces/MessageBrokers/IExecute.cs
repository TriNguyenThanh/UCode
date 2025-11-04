using AssignmentService.Domain.Entities;

namespace AssignmentService.Application.Interfaces.MessageBrokers;

public interface IExecuteService
{
    Task ExecuteCode(Submission submission);
}