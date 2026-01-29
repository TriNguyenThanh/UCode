using System.Text;
using System.Text.RegularExpressions;
using AssignmentService.Application.Interfaces.Services;
using AssignmentService.Application.Interfaces.Repositories;
using AssignmentService.Domain.Entities;
using Microsoft.Extensions.Logging;
using AssignmentService.Application.Interfaces.MessageBrokers;
using AssignmentService.Application.DTOs.Requests;

namespace AssignmentService.Infrastructure.Services;

/// <summary>
/// Service for formatting source code across multiple programming languages
/// </summary>
public class CodeFormatterService : ICodeFormatterService
{
    private readonly ILogger<CodeFormatterService> _logger;
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IRabbitMqService _rabbitMqService;
    public CodeFormatterService(
        ILogger<CodeFormatterService> logger,
        ISubmissionRepository submissionRepository,
        IRabbitMqService rabbitMqService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
        _rabbitMqService = rabbitMqService ?? throw new ArgumentNullException(nameof(rabbitMqService));
    }

    /// <summary>
    /// Format code for a submission by retrieving it from database
    /// </summary>
    /// <param name="submissionId">The submission GUID</param>
    /// <returns>The submission with formatted source code</returns>
    public async Task<Submission> FormatCode(Guid submissionId)
    {
        try
        {
            // _logger.LogInformation("Fetching submission {SubmissionId} for formatting", submissionId);

            var submission = await _submissionRepository.GetSubmission(submissionId);

            if (submission == null)
            {
                // _logger.LogWarning("Submission {SubmissionId} not found", submissionId);
                throw new ArgumentException($"Submission {submissionId} not found", nameof(submissionId));
            }

            if (string.IsNullOrWhiteSpace(submission.SourceCode))
            {
                // _logger.LogWarning("Submission {SubmissionId} has empty source code", submissionId);
                return submission;
            }

            // _logger.LogInformation("Formatting code for submission {SubmissionId}, language: {Language}", 
            //     submissionId, submission.LanguageCode);

            // Format the source code
            submission.SourceCode = await FormatCodeDirect(submission.SourceCode, submission.LanguageCode);

            // Update submission in database with formatted code
            await _submissionRepository.UpdateSubmission(submission);

            _logger.LogInformation("Successfully formatted and updated submission {SubmissionId}", submissionId);

            return submission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to format submission {SubmissionId}", submissionId);
            throw;
        }
    }

    /// <summary>
    /// Format code directly with code string and language
    /// </summary>
    /// <param name="code">The source code to format</param>
    /// <param name="language">Language code (cpp, python, java, javascript, c, etc.)</param>
    /// <returns>Formatted code</returns>
    public async Task<string> FormatCodeDirect(string code, string language)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            _logger.LogWarning("Received empty code for formatting");
            return code;
        }

        if (string.IsNullOrWhiteSpace(language))
        {
            _logger.LogWarning("Received empty language for formatting");
            return code;
        }

        try
        {
            _logger.LogInformation("Formatting code for language: {Language}", language);

            var normalizedLanguage = language.ToLowerInvariant().Trim();

            // Apply language-specific formatting
            var formattedCode = normalizedLanguage switch
            {
                "cpp" or "c++" or "c" => await FormatCppCode(code),
                "python" or "py" => await FormatPythonCode(code),
                "java" => await FormatJavaCode(code),
                "javascript" or "js" => await FormatJavaScriptCode(code),
                "csharp" or "cs" or "c#" => await FormatCSharpCode(code),
                _ => await FormatGenericCode(code)
            };

            _logger.LogInformation("Code formatted successfully for language: {Language}", language);
            return formattedCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to format code for language: {Language}", language);
            // Return original code if formatting fails
            return code;
        }
    }

    /// <summary>
    /// Send code formatting request to RabbitMQ
    /// </summary>
    /// <param name="submissionId">The submission GUID</param>
    /// <returns>Task</returns>
    /// <remarks>This method enqueues a message to RabbitMQ for asynchronous processing.</remarks>
    /// <exception cref="ArgumentException">Thrown when submissionId is empty.</exception>
    public async Task<bool> EnqueueCodeFormattingRequest(Submission submission)
    {
        try
        {
            if (submission == null)
            {
                throw new ArgumentNullException(nameof(submission), "Submission cannot be null.");
            }

            var message = new FormatCodeMessage
            {
                SubmissionId = submission.SubmissionId
            };

            _logger.LogInformation("Enqueuing code formatting request for submission {SubmissionId}", submission.SubmissionId);
            await _rabbitMqService.DeclareQueueAsync("code_formatter_queue");
            await _rabbitMqService.PublishMessageAsync(
                queueName: "code_formatter_queue",
                message: message);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue code formatting request for submission {SubmissionId}", submission.SubmissionId);
            throw;
        }
    }

    #region C++ Formatting

    private async Task<string> FormatCppCode(string code)
    {
        return await Task.Run(() =>
        {
            // Step 1: Remove comments
            code = RemoveCppComments(code);

            // Step 2: Normalize code structure
            var sb = new StringBuilder();
            var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Skip empty lines completely for canonical form
                if (string.IsNullOrWhiteSpace(trimmedLine))
                    continue;

                // Normalize whitespace: replace multiple spaces with single space
                trimmedLine = NormalizeWhitespace(trimmedLine);

                sb.AppendLine(trimmedLine);
            }

            return sb.ToString().Trim();
        });
    }

    #endregion

    #region Python Formatting

    private async Task<string> FormatPythonCode(string code)
    {
        return await Task.Run(() =>
        {
            // Step 1: Remove comments
            code = RemovePythonComments(code);

            // Step 2: Normalize code structure
            var sb = new StringBuilder();
            var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Skip empty lines completely for canonical form
                if (string.IsNullOrWhiteSpace(trimmedLine))
                    continue;

                // Normalize whitespace
                trimmedLine = NormalizeWhitespace(trimmedLine);

                sb.AppendLine(trimmedLine);
            }

            return sb.ToString().Trim();
        });
    }

    #endregion

    #region Java Formatting

    private async Task<string> FormatJavaCode(string code)
    {
        return await Task.Run(() =>
        {
            // Step 1: Remove comments
            code = RemoveCppComments(code); // Java uses same comment style as C++

            // Step 2: Normalize code structure
            var sb = new StringBuilder();
            var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Skip empty lines completely for canonical form
                if (string.IsNullOrWhiteSpace(trimmedLine))
                    continue;

                // Normalize whitespace
                trimmedLine = NormalizeWhitespace(trimmedLine);

                sb.AppendLine(trimmedLine);
            }

            return sb.ToString().Trim();
        });
    }

    #endregion

    #region JavaScript Formatting

    private async Task<string> FormatJavaScriptCode(string code)
    {
        return await Task.Run(() =>
        {
            // Step 1: Remove comments
            code = RemoveCppComments(code); // JavaScript uses same comment style as C++

            // Step 2: Normalize code structure
            var sb = new StringBuilder();
            var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Skip empty lines completely for canonical form
                if (string.IsNullOrWhiteSpace(trimmedLine))
                    continue;

                // Normalize whitespace
                trimmedLine = NormalizeWhitespace(trimmedLine);

                sb.AppendLine(trimmedLine);
            }

            return sb.ToString().Trim();
        });
    }

    #endregion

    #region C# Formatting

    private async Task<string> FormatCSharpCode(string code)
    {
        return await Task.Run(() =>
        {
            // Step 1: Remove comments
            code = RemoveCppComments(code); // C# uses same comment style as C++

            // Step 2: Normalize code structure
            var sb = new StringBuilder();
            var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Skip empty lines completely for canonical form
                if (string.IsNullOrWhiteSpace(trimmedLine))
                    continue;

                // Normalize whitespace
                trimmedLine = NormalizeWhitespace(trimmedLine);

                sb.AppendLine(trimmedLine);
            }

            return sb.ToString().Trim();
        });
    }

    #endregion

    #region Generic Formatting

    private async Task<string> FormatGenericCode(string code)
    {
        return await Task.Run(() =>
        {
            // Basic formatting: normalize line endings and trim trailing whitespace
            var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var formattedLines = lines.Select(line => line.TrimEnd());
            return string.Join("\n", formattedLines);
        });
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Remove C++/C/Java/C#/JavaScript style comments (// and /* */)
    /// </summary>
    private string RemoveCppComments(string code)
    {
        // Remove multi-line comments /* ... */
        code = Regex.Replace(code, @"/\*.*?\*/", "", RegexOptions.Singleline);

        // Remove single-line comments //
        code = Regex.Replace(code, @"//.*?$", "", RegexOptions.Multiline);

        return code;
    }

    /// <summary>
    /// Remove Python comments (#)
    /// </summary>
    private string RemovePythonComments(string code)
    {
        var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var result = new List<string>();

        foreach (var line in lines)
        {
            // Find # that's not inside a string
            var cleanLine = line;
            var commentIndex = -1;
            bool inString = false;
            char stringChar = '\0';

            for (int i = 0; i < line.Length; i++)
            {
                if (!inString && (line[i] == '"' || line[i] == '\''))
                {
                    inString = true;
                    stringChar = line[i];
                }
                else if (inString && line[i] == stringChar && (i == 0 || line[i - 1] != '\\'))
                {
                    inString = false;
                }
                else if (!inString && line[i] == '#')
                {
                    commentIndex = i;
                    break;
                }
            }

            if (commentIndex >= 0)
            {
                cleanLine = line.Substring(0, commentIndex);
            }

            result.Add(cleanLine);
        }

        return string.Join("\n", result);
    }

    /// <summary>
    /// Normalize whitespace: replace multiple spaces/tabs with single space
    /// </summary>
    private string NormalizeWhitespace(string line)
    {
        // Replace tabs with spaces
        line = line.Replace("\t", " ");

        // Replace multiple spaces with single space
        line = Regex.Replace(line, @"\s+", " ");

        return line.Trim();
    }



    #endregion
}