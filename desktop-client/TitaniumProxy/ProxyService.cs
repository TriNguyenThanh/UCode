using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;

namespace TitaniumProxy;

/// <summary>
/// Proxy service for WPF/library integration
/// Provides easy-to-use API for starting/stopping proxy and getting statistics
/// </summary>
public class ProxyService : IDisposable
{
    private ProxyServer? _proxyServer;
    private TrafficLogger? _trafficLogger;
    private DnsCacheMonitor? _dnsMonitor;
    private ProxyWatchdog? _watchdog;
    private WhitelistManager? _whitelistManager;

    private readonly object _lock = new();
    private bool _isRunning;
    private int _port = 8888;
    private ProxyMode _proxyMode = ProxyMode.Detect;

    /// <summary>
    /// Whether the proxy is currently running
    /// </summary>
    public bool IsRunning => _isRunning;

    /// <summary>
    /// Current proxy port
    /// </summary>
    public int Port => _port;

    /// <summary>
    /// Current proxy mode
    /// </summary>
    public ProxyMode Mode => _proxyMode;

    /// <summary>
    /// Number of times proxy was auto re-enabled by watchdog
    /// </summary>
    public int WatchdogReEnableCount => _watchdog?.ReEnableCount ?? 0;

    /// <summary>
    /// Event fired when an AI service is detected
    /// </summary>
    public event Action<string, string>? OnAiDetected; // (service, hostname)

    /// <summary>
    /// Event fired when proxy is re-enabled by watchdog
    /// </summary>
    public event Action<int>? OnProxyReEnabled; // (count)

    /// <summary>
    /// Start the proxy service
    /// </summary>
    /// <param name="port">Proxy port (default: 8888)</param>
    /// <param name="filterAi">Filter AI hosts only (default: true)</param>
    /// <param name="mode">Proxy mode: Detect or Block</param>
    /// <param name="whitelistHosts">Whitelist hosts for block mode (list of hosts)</param>
    /// <param name="whitelistFilePath">Path to whitelist file (overrides whitelistHosts if provided)</param>
    /// <param name="autoInstallCert">Auto install certificate</param>
    /// <returns>True if started successfully</returns>
    public bool Start(
        int port = 8888,
        bool filterAi = true,
        ProxyMode mode = ProxyMode.Detect,
        List<string>? whitelistHosts = null,
        string? whitelistFilePath = null,
        bool autoInstallCert = true)
    {
        lock (_lock)
        {
            if (_isRunning)
                return false;

            try
            {
                _port = port;
                _proxyMode = mode;

                // Load whitelist from file if path provided (takes precedence)
                if (!string.IsNullOrEmpty(whitelistFilePath))
                {
                    whitelistHosts = LoadWhitelistFromFile(whitelistFilePath);
                }

                // Initialize whitelist if in block mode
                if (mode == ProxyMode.Block)
                {
                    _whitelistManager = new WhitelistManager(whitelistHosts ?? new List<string>());
                }

                // Enable system proxy
                if (!SystemProxyManager.EnableProxy(port: port))
                    return false;

                // Initialize traffic logger
                _trafficLogger = new TrafficLogger(filterAi, debugMode: false);

                // Initialize proxy server
                _proxyServer = new ProxyServer();
                _proxyServer.CertificateManager.SaveFakeCertificates = true;
                _proxyServer.CertificateManager.RootCertificateName = "Titanium Root Certificate Authority";
                _proxyServer.CertificateManager.RootCertificateIssuerName = "Titanium Web Proxy";
                _proxyServer.CertificateManager.EnsureRootCertificate();

                // Install certificate if needed
                if (autoInstallCert && !CertificateManager.IsInstalled())
                {
                    var rootCert = _proxyServer.CertificateManager.RootCertificate;
                    if (rootCert != null)
                    {
                        CertificateManager.InstallSilent(rootCert);
                        var certPath = CertificateManager.GetCertPath();
                        CertificateManager.ExportToFile(rootCert, certPath);
                    }
                }

                // Subscribe to events
                _proxyServer.BeforeRequest += OnRequest;
                _proxyServer.BeforeResponse += OnResponse;
                _proxyServer.ServerCertificateValidationCallback += OnCertificateValidation;

                // Set up endpoint
                var explicitEndPoint = new ExplicitProxyEndPoint(System.Net.IPAddress.Any, port, true);
                _proxyServer.AddEndPoint(explicitEndPoint);

                // Start proxy
                _proxyServer.Start();

                // Clear DNS cache
                DnsCacheMonitor.ClearDnsCache();

                // Start watchdog
                _watchdog = new ProxyWatchdog(port);
                _watchdog.OnProxyReEnabled += count => OnProxyReEnabled?.Invoke(count);
                _watchdog.Start();

                // Start DNS monitor
                _dnsMonitor = new DnsCacheMonitor(_trafficLogger);
                _dnsMonitor.OnNewAiHostDetected += (service, hostname) => OnAiDetected?.Invoke(service, hostname);
                _dnsMonitor.Start();

                _isRunning = true;
                return true;
            }
            catch (Exception)
            {
                Stop();
                return false;
            }
        }
    }

    /// <summary>
    /// Load whitelist hosts from file
    /// Format: one host per line, lines starting with # are comments
    /// Supports wildcards: *.example.com
    /// </summary>
    public static List<string> LoadWhitelistFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            return new List<string>();

        return File.ReadAllLines(filePath)
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith("#"))
            .Select(line => line.Trim())
            .ToList();
    }

    /// <summary>
    /// Stop the proxy service
    /// </summary>
    public void Stop()
    {
        lock (_lock)
        {
            if (!_isRunning && _proxyServer == null)
                return;

            // Stop monitors
            _watchdog?.Stop();
            _dnsMonitor?.Stop();

            // Stop proxy
            if (_proxyServer != null)
            {
                _proxyServer.BeforeRequest -= OnRequest;
                _proxyServer.BeforeResponse -= OnResponse;
                _proxyServer.Stop();
                _proxyServer.Dispose();
                _proxyServer = null;
            }

            // Disable system proxy
            SystemProxyManager.DisableProxy();

            _isRunning = false;
        }
    }

    /// <summary>
    /// Get current detection statistics
    /// </summary>
    public Dictionary<string, int> GetStats()
    {
        return _trafficLogger?.GetStats() ?? new Dictionary<string, int>();
    }

    /// <summary>
    /// Get detailed statistics with DNS/Proxy breakdown
    /// </summary>
    public Dictionary<string, (int Total, int DNS, int Proxy, int UniqueHosts)> GetDetailedStats()
    {
        return _trafficLogger?.GetDetailedStats() ?? new Dictionary<string, (int, int, int, int)>();
    }

    /// <summary>
    /// Get statistics and clear counters (for sending to server)
    /// </summary>
    public Dictionary<string, int> GetStatsAndClear()
    {
        return _trafficLogger?.GetStatsAndClear() ?? new Dictionary<string, int>();
    }

    /// <summary>
    /// Get detailed statistics and clear counters
    /// </summary>
    public Dictionary<string, (int Total, int DNS, int Proxy, int UniqueHosts)> GetDetailedStatsAndClear()
    {
        return _trafficLogger?.GetDetailedStatsAndClear() ?? new Dictionary<string, (int, int, int, int)>();
    }

    /// <summary>
    /// Clear all statistics
    /// </summary>
    public void ClearStats()
    {
        _trafficLogger?.ClearStats();
    }

    /// <summary>
    /// Get total requests count
    /// </summary>
    public int GetTotalRequests()
    {
        return _trafficLogger?.GetTotalRequests() ?? 0;
    }

    /// <summary>
    /// Check if system proxy is currently enabled
    /// </summary>
    public bool IsSystemProxyEnabled()
    {
        return SystemProxyManager.IsProxyEnabled();
    }

    private Task OnRequest(object sender, SessionEventArgs e)
    {
        var host = e.HttpClient.Request.RequestUri.Host;
        var method = e.HttpClient.Request.Method;
        var url = e.HttpClient.Request.Url;

        // Block mode logic
        if (_proxyMode == ProxyMode.Block)
        {
            bool isWhitelisted = _whitelistManager?.IsWhitelisted(host) ?? false;

            if (!isWhitelisted)
            {
                var blockMessage = $@"
                    <!DOCTYPE html>
                    <html>
                    <head><title>Blocked</title></head>
                    <body style='font-family: Arial; text-align: center; padding: 50px;'>
                    <h1 style='color: red;'>Access Blocked</h1>
                    <h2>{host}</h2>
                    <p>This host has been blocked by Ucode.</p>
                    <hr>
                    <small>Ucode - Block Mode</small>
                    </body>
                    </html>";

                e.GenericResponse(blockMessage, System.Net.HttpStatusCode.Forbidden);
                return Task.CompletedTask;
            }
        }

        // Log detection
        _trafficLogger?.OnRequest(host, method, url);

        return Task.CompletedTask;
    }

    private Task OnResponse(object sender, SessionEventArgs e)
    {
        // Currently no action needed on response
        return Task.CompletedTask;
    }

    private Task OnCertificateValidation(object sender, CertificateValidationEventArgs e)
    {
        if (e.SslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
        {
            e.IsValid = true;
        }
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }
}
