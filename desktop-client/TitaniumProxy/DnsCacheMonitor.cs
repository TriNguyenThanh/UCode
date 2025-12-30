using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace TitaniumProxy;

/// <summary>
/// Monitor Windows DNS cache for AI service usage (fallback monitoring)
/// </summary>
public class DnsCacheMonitor
{
    private readonly TrafficLogger _trafficLogger;
    private readonly ConcurrentDictionary<string, bool> _seenHosts = new();
    private readonly CancellationTokenSource _cts = new();
    private Task? _monitorTask;
    private bool _isRunning;

    public event Action<string, string>? OnNewAiHostDetected; // (service, hostname)

    public DnsCacheMonitor(TrafficLogger trafficLogger)
    {
        _trafficLogger = trafficLogger;
    }

    /// <summary>
    /// Start monitoring DNS cache
    /// </summary>
    public void Start()
    {
        if (_isRunning)
            return;

        _isRunning = true;
        _monitorTask = Task.Run(() => MonitorLoop(_cts.Token), _cts.Token);
        Console.WriteLine("[DNS Monitor] Started (fallback mode)");
    }

    /// <summary>
    /// Stop monitoring
    /// </summary>
    public void Stop()
    {
        if (!_isRunning)
            return;

        _isRunning = false;
        _cts.Cancel();
        _monitorTask?.Wait(TimeSpan.FromSeconds(5));
        Console.WriteLine("[DNS Monitor] Stopped");
    }

    /// <summary>
    /// Monitor loop - runs in background
    /// </summary>
    private async Task MonitorLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var hostnames = GetDnsCacheHostnames();

                foreach (var hostname in hostnames)
                {
                    if (_seenHosts.ContainsKey(hostname))
                        continue;

                    _seenHosts[hostname] = true;

                    // Check if AI-related
                    if (IsAiRelated(hostname))
                    {
                        var service = GetServiceName(hostname);
                        
                        // Log detection via TrafficLogger
                        _trafficLogger.LogDetection("dns", service, hostname);
                        
                        // Fire event for console notification
                        OnNewAiHostDetected?.Invoke(service, hostname);
                    }
                }

                await Task.Delay(500, ct); // Check every 500ms
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                // Ignore errors, continue monitoring
                await Task.Delay(1000, ct);
            }
        }
    }

    /// <summary>
    /// Get all hostnames from Windows DNS cache
    /// </summary>
    private List<string> GetDnsCacheHostnames()
    {
        var hostnames = new List<string>();

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "ipconfig",
                Arguments = "/displaydns",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
                return hostnames;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Pattern to match "Record Name . . . . . : api.openai.com"
            var pattern = @"Record Name.*?:\s*(.+)";
            var matches = Regex.Matches(output, pattern);

            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    var hostname = match.Groups[1].Value.Trim();
                    if (!string.IsNullOrWhiteSpace(hostname))
                    {
                        hostnames.Add(hostname);
                    }
                }
            }
        }
        catch
        {
            // Ignore errors
        }

        return hostnames;
    }

    /// <summary>
    /// Check if hostname is AI-related (reuse TrafficLogger logic)
    /// </summary>
    private bool IsAiRelated(string hostname)
    {
        var hostLower = hostname.ToLower();

        // Exclude patterns
        var excludePatterns = new[] { "monitor.azure", "privatelink", "telemetry", "analytics", "cdn" };
        foreach (var exclude in excludePatterns)
        {
            if (hostLower.Contains(exclude))
                return false;
        }

        // AI keywords from TrafficLogger
        var aiKeywords = new[]
        {
            "openai", "chatgpt", "anthropic", "claude", "gemini", "generativelanguage",
            "cursor", "kiro", "githubcopilot", "cohere", "huggingface", "stability",
            "replicate", "perplexity", "character.ai", "blackbox", "phind", "deepseek",
            "copilot", "bard", "poe.com", "you.com"
        };

        foreach (var keyword in aiKeywords)
        {
            if (hostLower.Contains(keyword))
                return true;
        }


        return false;
    }

    /// <summary>
    /// Get service name from hostname (reuse TrafficLogger logic)
    /// </summary>
    private string GetServiceName(string hostname)
    {
        var hostLower = hostname.ToLower();

        if (hostLower.Contains("openai") || hostLower.Contains("chatgpt"))
            return "OpenAI";
        if (hostLower.Contains("anthropic") || hostLower.Contains("claude"))
            return "Claude";
        if (hostLower.Contains("gemini") || hostLower.Contains("generativelanguage"))
            return "Gemini";
        if (hostLower.Contains("cursor"))
            return "Cursor";
        if (hostLower.Contains("kiro"))
            return "Kiro";
        if (hostLower.Contains("githubcopilot") || (hostLower.Contains("copilot") && hostLower.Contains("github")))
            return "GitHub Copilot";
        if (hostLower.Contains("copilot"))
            return "Microsoft Copilot";
        if (hostLower.Contains("deepseek"))
            return "DeepSeek";

        // Fallback
        var parts = hostname.Split('.');
        if (parts.Length >= 2)
        {
            var domain = string.Join(".", parts.TakeLast(2));
            return $"Unknown: {domain}";
        }

        return $"Unknown: {hostname}";
    }

    /// <summary>
    /// Clear Windows DNS cache
    /// </summary>
    public static bool ClearDnsCache()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "ipconfig",
                Arguments = "/flushdns",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
                return false;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output.Contains("Successfully flushed", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
