using System.Text;
using System.Text.RegularExpressions;
using AssignmentService.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AssignmentService.Infrastructure.Services;

/// <summary>
/// Service for formatting source code across multiple programming languages
/// </summary>
public class CodeFormatterService : ICodeFormatterService
{
    private readonly ILogger<CodeFormatterService> _logger;

    public CodeFormatterService(ILogger<CodeFormatterService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Format code based on the language
    /// </summary>
    /// <param name="code">The source code to format</param>
    /// <param name="language">Language code (cpp, python, java, javascript, c, etc.)</param>
    /// <returns>Formatted code</returns>
    public async Task<string> FormatCode(string code, string language)
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

            _logger.LogInformation("✅ Code formatted successfully for language: {Language}", language);
            return formattedCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to format code for language: {Language}", language);
            // Return original code if formatting fails
            return code;
        }
    }

    #region C++ Formatting

    private async Task<string> FormatCppCode(string code)
    {
        return await Task.Run(() =>
        {
            var sb = new StringBuilder();
            var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int indentLevel = 0;
            const string indent = "    "; // 4 spaces

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(trimmedLine))
                {
                    sb.AppendLine();
                    continue;
                }

                // Decrease indent for closing braces
                if (trimmedLine.StartsWith("}"))
                {
                    indentLevel = Math.Max(0, indentLevel - 1);
                }

                // Add proper indentation
                var currentIndent = new string(' ', indentLevel * indent.Length);
                sb.AppendLine(currentIndent + trimmedLine);

                // Increase indent for opening braces
                if (trimmedLine.EndsWith("{"))
                {
                    indentLevel++;
                }
                // Handle closing and opening brace on same line (e.g., "} else {")
                else if (trimmedLine.Contains("} else {") || trimmedLine.Contains("} catch") || trimmedLine.Contains("} while"))
                {
                    indentLevel++;
                }
            }

            return FormatWhitespace(sb.ToString());
        });
    }

    #endregion

    #region Python Formatting

    private async Task<string> FormatPythonCode(string code)
    {
        return await Task.Run(() =>
        {
            var sb = new StringBuilder();
            var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int indentLevel = 0;
            const string indent = "    "; // PEP 8: 4 spaces

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(trimmedLine))
                {
                    sb.AppendLine();
                    continue;
                }

                // Decrease indent for dedent keywords
                if (IsPythonDedent(trimmedLine))
                {
                    indentLevel = Math.Max(0, indentLevel - 1);
                }

                // Add proper indentation
                var currentIndent = new string(' ', indentLevel * indent.Length);
                sb.AppendLine(currentIndent + trimmedLine);

                // Increase indent after colons (function/class/if/for/while definitions)
                if (trimmedLine.EndsWith(":") && !trimmedLine.TrimStart().StartsWith("#"))
                {
                    indentLevel++;
                }
            }

            return FormatWhitespace(sb.ToString());
        });
    }

    private bool IsPythonDedent(string line)
    {
        var keywords = new[] { "elif ", "else:", "except ", "except:", "finally:", "return ", "break", "continue", "pass" };
        return keywords.Any(keyword => line.StartsWith(keyword));
    }

    #endregion

    #region Java Formatting

    private async Task<string> FormatJavaCode(string code)
    {
        return await Task.Run(() =>
        {
            var sb = new StringBuilder();
            var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int indentLevel = 0;
            const string indent = "    "; // 4 spaces

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(trimmedLine))
                {
                    sb.AppendLine();
                    continue;
                }

                // Decrease indent for closing braces
                if (trimmedLine.StartsWith("}"))
                {
                    indentLevel = Math.Max(0, indentLevel - 1);
                }

                // Add proper indentation
                var currentIndent = new string(' ', indentLevel * indent.Length);
                sb.AppendLine(currentIndent + trimmedLine);

                // Increase indent for opening braces
                if (trimmedLine.EndsWith("{"))
                {
                    indentLevel++;
                }
                // Handle closing and opening brace on same line
                else if (trimmedLine.Contains("} else {") || trimmedLine.Contains("} catch") || trimmedLine.Contains("} finally"))
                {
                    indentLevel++;
                }
            }

            return FormatWhitespace(sb.ToString());
        });
    }

    #endregion

    #region JavaScript Formatting

    private async Task<string> FormatJavaScriptCode(string code)
    {
        return await Task.Run(() =>
        {
            var sb = new StringBuilder();
            var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int indentLevel = 0;
            const string indent = "  "; // 2 spaces (common JS convention)

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(trimmedLine))
                {
                    sb.AppendLine();
                    continue;
                }

                // Decrease indent for closing braces/brackets
                if (trimmedLine.StartsWith("}") || trimmedLine.StartsWith("]") || trimmedLine.StartsWith(")"))
                {
                    indentLevel = Math.Max(0, indentLevel - 1);
                }

                // Add proper indentation
                var currentIndent = new string(' ', indentLevel * indent.Length);
                sb.AppendLine(currentIndent + trimmedLine);

                // Increase indent for opening braces/brackets
                if (trimmedLine.EndsWith("{") || trimmedLine.EndsWith("["))
                {
                    indentLevel++;
                }
                // Handle arrow functions
                else if (trimmedLine.Contains("=>") && trimmedLine.EndsWith("{"))
                {
                    indentLevel++;
                }
            }

            return FormatWhitespace(sb.ToString());
        });
    }

    #endregion

    #region C# Formatting

    private async Task<string> FormatCSharpCode(string code)
    {
        return await Task.Run(() =>
        {
            var sb = new StringBuilder();
            var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int indentLevel = 0;
            const string indent = "    "; // 4 spaces

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(trimmedLine))
                {
                    sb.AppendLine();
                    continue;
                }

                // Decrease indent for closing braces
                if (trimmedLine.StartsWith("}"))
                {
                    indentLevel = Math.Max(0, indentLevel - 1);
                }

                // Add proper indentation
                var currentIndent = new string(' ', indentLevel * indent.Length);
                sb.AppendLine(currentIndent + trimmedLine);

                // Increase indent for opening braces
                if (trimmedLine.EndsWith("{"))
                {
                    indentLevel++;
                }
                // Handle LINQ queries and properties
                else if (trimmedLine.StartsWith("get {") || trimmedLine.StartsWith("set {"))
                {
                    indentLevel++;
                }
            }

            return FormatWhitespace(sb.ToString());
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
    /// Format whitespace: normalize line endings and remove excessive blank lines
    /// </summary>
    private string FormatWhitespace(string code)
    {
        // Normalize line endings to \n
        code = code.Replace("\r\n", "\n").Replace("\r", "\n");

        // Remove trailing whitespace from each line
        var lines = code.Split('\n');
        lines = lines.Select(line => line.TrimEnd()).ToArray();

        // Remove excessive blank lines (max 2 consecutive blank lines)
        var result = new List<string>();
        int consecutiveBlankLines = 0;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                consecutiveBlankLines++;
                if (consecutiveBlankLines <= 2)
                {
                    result.Add(line);
                }
            }
            else
            {
                consecutiveBlankLines = 0;
                result.Add(line);
            }
        }

        // Trim trailing blank lines
        while (result.Count > 0 && string.IsNullOrWhiteSpace(result[^1]))
        {
            result.RemoveAt(result.Count - 1);
        }

        return string.Join("\n", result);
    }

    #endregion
} 