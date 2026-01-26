using AssignmentService.Domain.Entities;

namespace AssignmentService.Application.Interfaces.Services
{
    public interface ICodeFormatterService
    {
        /// <summary>
        /// Format code for a submission by retrieving it from database
        /// </summary>
        /// <param name="submissionId">The submission GUID</param>
        /// <returns>The submission with formatted source code</returns>
        Task<Submission> FormatCode(Guid submissionId);

        /// <summary>
        /// Format code directly with code string and language
        /// </summary>
        /// <param name="code">The source code to format</param>
        /// <param name="language">Language code (cpp, python, java, etc.)</param>
        /// <returns>Formatted code string</returns>
        Task<string> FormatCodeDirect(string code, string language);
        
        /// <summary>
        /// Send code formatting request to RabbitMQ
        /// </summary>
        /// <param name="submissionId">The submission GUID</param>
        /// <returns>Task</returns>
        /// <remarks>This method enqueues a message to RabbitMQ for asynchronous processing.</remarks>
        /// <exception cref="ArgumentException">Thrown when submissionId is empty.</exception>
        Task<bool> EnqueueCodeFormattingRequest(Submission submission);
    }
}