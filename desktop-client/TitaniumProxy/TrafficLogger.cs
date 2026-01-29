using System.Collections.Concurrent;

namespace TitaniumProxy;

/// <summary>
/// Log HTTP/HTTPS traffic with AI service counter
/// </summary>
public class TrafficLogger
{
    // Keyword mapping: (keywords, service_name, priority)
    // Priority: higher = check first (for specific matches)
    private static readonly List<(string[] Keywords, string ServiceName, int Priority)> ServiceKeywords = new()
    {
        // Specific matches first (higher priority)
        (new[] { "githubcopilot", "copilot-proxy.githubusercontent" }, "GitHub Copilot", 100),
        (new[] { "sydney.bing", "edgeservices.bing" }, "Microsoft Copilot", 90),
        (new[] { "copilot.microsoft.com" }, "Microsoft Copilot", 85),
        (new[] { "generativelanguage" }, "Gemini", 80),

        // General matches
        (new[] { "openai", "chatgpt" }, "OpenAI", 50),
        (new[] { "anthropic", "claude" }, "Claude", 50),
        (new[] { "deepseek" }, "DeepSeek", 50),
        (new[] { "gemini" }, "Gemini", 50),
        (new[] { "bard" }, "Bard", 50),
        (new[] { "cohere" }, "Cohere", 50),
        (new[] { "huggingface" }, "HuggingFace", 50),
        (new[] { "stability" }, "Stability AI", 50),
        (new[] { "cursor" }, "Cursor", 50),
        (new[] { "replicate" }, "Replicate", 50),
        (new[] { "perplexity" }, "Perplexity", 50),
        (new[] { "character.ai" }, "Character.AI", 50),
        (new[] { "blackbox" }, "Blackbox", 50),
        (new[] { "kiro" }, "Kiro", 50),
        (new[] { "phind" }, "Phind", 50),

        // Low priority (common domains)
        (new[] { "poe.com" }, "Poe", 10),
        (new[] { "you.com" }, "You.com", 10),
    };

    private readonly bool _filterAi;
    private readonly bool _debugMode;

    // Combined counters (total = DNS + Proxy)
    private readonly ConcurrentDictionary<string, int> _aiCounters = new();

    // Separate source counters
    private readonly ConcurrentDictionary<string, int> _dnsCounters = new();
    private readonly ConcurrentDictionary<string, int> _proxyCounters = new();
    private readonly ConcurrentDictionary<string, HashSet<string>> _uniqueHosts = new();

    private int _totalRequests = 0;
    private readonly object _consoleLock = new();

    public TrafficLogger(bool filterAi = true, bool debugMode = false)
    {
        _filterAi = filterAi;
        _debugMode = debugMode;
    }

    /// <summary>
    /// Get service name from host using keyword matching
    /// </summary>
    private string GetServiceName(string host)
    {
        var hostLower = host.ToLower();

        // Check keywords in priority order (sorted by priority descending)
        foreach (var (keywords, serviceName, _) in ServiceKeywords.OrderByDescending(x => x.Priority))
        {
            foreach (var keyword in keywords)
            {
                if (hostLower.Contains(keyword))
                {
                    return serviceName;
                }
            }
        }

        // Fallback: use domain name
        var parts = hostLower.Split('.');
        if (parts.Length > 0)
        {
            return TitleCase(parts[0]);
        }

        return hostLower;
    }

    /// <summary>
    /// Convert to title case
    /// </summary>
    private string TitleCase(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return char.ToUpper(text[0]) + text.Substring(1).ToLower();
    }

    /// <summary>
    /// Check if host is AI-related
    /// </summary>
    private bool IsAiHost(string host)
    {
        var hostLower = host.ToLower();

        foreach (var (keywords, _, _) in ServiceKeywords)
        {
            foreach (var keyword in keywords)
            {
                if (hostLower.Contains(keyword))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Print stats on same line (update in place) - format like ai_detector_Ucode.py
    /// </summary>
    private void PrintStats()
    {
        if (!_aiCounters.Any())
        {
            Console.Write($"\r[{DateTime.Now:HH:mm:ss}] No AI services detected yet...");
            return;
        }

        // Get top 3 services by total count
        var top3 = _aiCounters
            .OrderByDescending(x => x.Value)
            .Take(3)
            .Select(x =>
            {
                var service = x.Key;
                var total = x.Value;
                var dnsCount = _dnsCounters.GetValueOrDefault(service, 0);
                var proxyCount = _proxyCounters.GetValueOrDefault(service, 0);
                return $"{service}: {total} (DNS:{dnsCount} Proxy:{proxyCount})";
            });

        var statsStr = string.Join(" | ", top3);

        // Clear line and print with timestamp (like Python version)
        Console.Write($"\r[{DateTime.Now:HH:mm:ss}] {statsStr}                    ");
    }

    /// <summary>
    /// Log detection from DNS or Proxy source
    /// </summary>
    public void LogDetection(string source, string serviceName, string hostname)
    {
        // Update combined counter
        _aiCounters.AddOrUpdate(serviceName, 1, (key, old) => old + 1);

        // Update source-specific counter
        if (source.Equals("dns", StringComparison.OrdinalIgnoreCase))
        {
            _dnsCounters.AddOrUpdate(serviceName, 1, (key, old) => old + 1);
        }
        else // proxy
        {
            _proxyCounters.AddOrUpdate(serviceName, 1, (key, old) => old + 1);
        }

        // Track unique hosts
        _uniqueHosts.AddOrUpdate(serviceName,
            new HashSet<string> { hostname },
            (key, existing) =>
            {
                lock (existing)
                {
                    existing.Add(hostname);
                }
                return existing;
            });
    }

    /// <summary>
    /// Called when a request is received from Proxy
    /// </summary>
    public void OnRequest(string host, string method, string url)
    {
        // Filter: only log AI-related hosts if filter enabled
        if (_filterAi && !IsAiHost(host))
        {
            return;
        }

        Interlocked.Increment(ref _totalRequests);

        // Get service name
        var serviceName = GetServiceName(host);

        // Log as proxy detection
        LogDetection("proxy", serviceName, host);

        // Print updated stats (only if not in debug mode)
        if (!_debugMode)
        {
            PrintStats();
        }
    }

    /// <summary>
    /// Log full request details (debug mode)
    /// </summary>
    public void LogFullRequest(string method, string url, Dictionary<string, string> headers, string? body)
    {
        if (!_debugMode)
            return;

        lock (_consoleLock)
        {
            Console.WriteLine();
            Console.WriteLine("=".PadRight(100, '='));
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"📤 REQUEST #{_totalRequests}");
            Console.ResetColor();
            Console.WriteLine("=".PadRight(100, '='));

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{method} {url}");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Headers:");
            Console.ResetColor();
            foreach (var header in headers.OrderBy(h => h.Key))
            {
                Console.WriteLine($"  {header.Key}: {header.Value}");
            }

            if (!string.IsNullOrEmpty(body))
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("Body:");
                Console.ResetColor();

                // Pretty print body (limit to 5000 chars)
                var displayBody = body.Length > 5000 ? body.Substring(0, 5000) + "\n... (truncated)" : body;
                Console.WriteLine(displayBody);
            }
            else
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("(No body)");
                Console.ResetColor();
            }

            Console.WriteLine("=".PadRight(100, '='));
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Log full response details (debug mode)
    /// </summary>
    public void LogFullResponse(string url, int statusCode, string statusDescription, Dictionary<string, string> headers, string? body)
    {
        if (!_debugMode)
            return;

        lock (_consoleLock)
        {
            Console.WriteLine("=".PadRight(100, '='));
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"📥 RESPONSE");
            Console.ResetColor();
            Console.WriteLine("=".PadRight(100, '='));

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"URL: {url}");
            Console.ResetColor();

            // Color code by status
            var statusColor = statusCode >= 200 && statusCode < 300 ? ConsoleColor.Green :
                             statusCode >= 300 && statusCode < 400 ? ConsoleColor.Yellow :
                             statusCode >= 400 && statusCode < 500 ? ConsoleColor.Red :
                             ConsoleColor.DarkRed;

            Console.ForegroundColor = statusColor;
            Console.WriteLine($"Status: {statusCode} {statusDescription}");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Headers:");
            Console.ResetColor();
            foreach (var header in headers.OrderBy(h => h.Key))
            {
                Console.WriteLine($"  {header.Key}: {header.Value}");
            }

            if (!string.IsNullOrEmpty(body))
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("Body:");
                Console.ResetColor();

                // Pretty print body (limit to 5000 chars)
                var displayBody = body.Length > 5000 ? body.Substring(0, 5000) + "\n... (truncated)" : body;

                // Try to pretty print JSON
                if (IsJson(body))
                {
                    try
                    {
                        var prettyJson = PrettyPrintJson(displayBody);
                        // Console.WriteLine(prettyJson);
                    }
                    catch
                    {
                        // Console.WriteLine(displayBody);
                    }
                }
                else
                {
                    // Console.WriteLine(displayBody);
                }
            }
            else
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("(No body)");
                Console.ResetColor();
            }

            Console.WriteLine("=".PadRight(100, '='));
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Check if string is JSON
    /// </summary>
    private bool IsJson(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        text = text.Trim();
        return (text.StartsWith("{") && text.EndsWith("}")) ||
               (text.StartsWith("[") && text.EndsWith("]"));
    }

    /// <summary>
    /// Simple JSON pretty printer
    /// </summary>
    private string PrettyPrintJson(string json)
    {
        // Very basic JSON formatting
        var indent = 0;
        var result = new System.Text.StringBuilder();
        var inString = false;
        var escaped = false;

        foreach (var ch in json)
        {
            if (escaped)
            {
                result.Append(ch);
                escaped = false;
                continue;
            }

            if (ch == '\\' && inString)
            {
                result.Append(ch);
                escaped = true;
                continue;
            }

            if (ch == '"')
            {
                inString = !inString;
                result.Append(ch);
                continue;
            }

            if (inString)
            {
                result.Append(ch);
                continue;
            }

            switch (ch)
            {
                case '{':
                case '[':
                    result.Append(ch);
                    result.AppendLine();
                    indent++;
                    result.Append(new string(' ', indent * 2));
                    break;
                case '}':
                case ']':
                    result.AppendLine();
                    indent--;
                    result.Append(new string(' ', indent * 2));
                    result.Append(ch);
                    break;
                case ',':
                    result.Append(ch);
                    result.AppendLine();
                    result.Append(new string(' ', indent * 2));
                    break;
                case ':':
                    result.Append(ch);
                    result.Append(' ');
                    break;
                default:
                    if (!char.IsWhiteSpace(ch))
                        result.Append(ch);
                    break;
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Get current statistics
    /// </summary>
    public Dictionary<string, int> GetStats()
    {
        return new Dictionary<string, int>(_aiCounters);
    }

    /// <summary>
    /// Get detailed statistics with DNS/Proxy breakdown
    /// </summary>
    public Dictionary<string, (int Total, int DNS, int Proxy, int UniqueHosts)> GetDetailedStats()
    {
        var result = new Dictionary<string, (int, int, int, int)>();

        foreach (var service in _aiCounters.Keys)
        {
            var total = _aiCounters.GetValueOrDefault(service, 0);
            var dns = _dnsCounters.GetValueOrDefault(service, 0);
            var proxy = _proxyCounters.GetValueOrDefault(service, 0);
            var uniqueCount = 0;

            if (_uniqueHosts.TryGetValue(service, out var hosts))
            {
                lock (hosts)
                {
                    uniqueCount = hosts.Count;
                }
            }

            result[service] = (total, dns, proxy, uniqueCount);
        }

        return result;
    }

    /// <summary>
    /// Get current stats and clear all counters (for app integration)
    /// Returns: Dictionary with service name as key, total count as value
    /// </summary>
    public Dictionary<string, int> GetStatsAndClear()
    {
        // Copy current stats
        var result = new Dictionary<string, int>(_aiCounters);

        // Clear all counters
        ClearStats();

        return result;
    }

    /// <summary>
    /// Get detailed stats and clear all counters (for app integration)
    /// Returns: Dictionary with service name as key, (Total, DNS, Proxy, UniqueHosts) as value
    /// </summary>
    public Dictionary<string, (int Total, int DNS, int Proxy, int UniqueHosts)> GetDetailedStatsAndClear()
    {
        // Get current stats
        var result = GetDetailedStats();

        // Clear all counters
        ClearStats();

        return result;
    }

    /// <summary>
    /// Clear all statistics counters
    /// </summary>
    public void ClearStats()
    {
        _aiCounters.Clear();
        _dnsCounters.Clear();
        _proxyCounters.Clear();
        _uniqueHosts.Clear();
        DnsCacheMonitor.ClearDnsCache();
        Interlocked.Exchange(ref _totalRequests, 0);
    }

    /// <summary>
    /// Get total requests count
    /// </summary>
    public int GetTotalRequests()
    {
        return _totalRequests;
    }

    /// <summary>
    /// Print summary statistics (like ai_detector_Ucode.py)
    /// </summary>
    public void PrintSummary()
    {
        Console.WriteLine();
        Console.WriteLine("=".PadRight(70, '='));
        Console.WriteLine("Summary");
        Console.WriteLine("=".PadRight(70, '='));
        Console.WriteLine();

        if (!_aiCounters.Any())
        {
            Console.WriteLine("No AI services detected");
            return;
        }

        // Header
        Console.WriteLine($"{"Service",-30} {"Total",-8} {"DNS",-8} {"Proxy",-8} {"Unique Hosts"}");
        Console.WriteLine("-".PadRight(70, '-'));

        // Services sorted by total count
        foreach (var kvp in _aiCounters.OrderByDescending(x => x.Value))
        {
            var service = kvp.Key;
            var total = kvp.Value;
            var dnsCount = _dnsCounters.GetValueOrDefault(service, 0);
            var proxyCount = _proxyCounters.GetValueOrDefault(service, 0);
            var uniqueCount = 0;

            if (_uniqueHosts.TryGetValue(service, out var hosts))
            {
                lock (hosts)
                {
                    uniqueCount = hosts.Count;
                }
            }

            Console.WriteLine($"{service,-30} {total,-8} {dnsCount,-8} {proxyCount,-8} {uniqueCount}");
        }

        Console.WriteLine();
    }
}
