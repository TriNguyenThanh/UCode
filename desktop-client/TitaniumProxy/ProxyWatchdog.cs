using Microsoft.Win32;

namespace TitaniumProxy;

/// <summary>
/// Watchdog that monitors and auto-restores system proxy if disabled
/// </summary>
public class ProxyWatchdog
{
    private readonly int _port;
    private readonly CancellationTokenSource _cts = new();
    private Task? _watchdogTask;
    private bool _isRunning;
    private int _reEnableCount = 0;
    private DateTime _lastCheck = DateTime.Now;

    public int ReEnableCount => _reEnableCount;

    public event Action<int>? OnProxyReEnabled; // (count)

    public ProxyWatchdog(int port = 8888)
    {
        _port = port;
    }

    /// <summary>
    /// Start watchdog monitoring
    /// </summary>
    public void Start()
    {
        if (_isRunning)
            return;

        _isRunning = true;
        _watchdogTask = Task.Run(() => WatchdogLoop(_cts.Token), _cts.Token);
        Console.WriteLine("[Watchdog] Started - monitoring proxy status");
    }

    /// <summary>
    /// Stop watchdog
    /// </summary>
    public void Stop()
    {
        if (!_isRunning)
            return;

        _isRunning = false;
        _cts.Cancel();
        _watchdogTask?.Wait(TimeSpan.FromSeconds(5));
        Console.WriteLine("[Watchdog] Stopped");
    }

    /// <summary>
    /// Watchdog loop - runs in background
    /// </summary>
    private async Task WatchdogLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;

                // Check every 5 seconds
                if ((now - _lastCheck).TotalSeconds >= 5)
                {
                    _lastCheck = now;

                    if (!IsProxyEnabled())
                    {
                        _reEnableCount++;
                        
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"⚠ [Watchdog] Proxy disabled detected! (#{_reEnableCount})");
                        Console.WriteLine("  → Re-enabling proxy...");
                        Console.ResetColor();

                        if (SystemProxyManager.EnableProxy(port: _port))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("  ✓ Proxy re-enabled");
                            Console.ResetColor();
                            OnProxyReEnabled?.Invoke(_reEnableCount);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("  ✗ Failed to re-enable proxy");
                            Console.ResetColor();
                        }
                    }
                }

                await Task.Delay(1000, ct); // Check every second
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
    /// Check if system proxy is enabled
    /// </summary>
    private bool IsProxyEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Internet Settings",
                writable: false);

            if (key == null)
                return false;

            var proxyEnable = key.GetValue("ProxyEnable");
            return proxyEnable is int value && value == 1;
        }
        catch
        {
            return false;
        }
    }
}
