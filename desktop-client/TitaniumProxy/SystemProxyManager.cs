using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace TitaniumProxy;

/// <summary>
/// Manage Windows system proxy settings
/// </summary>
public static class SystemProxyManager
{
    private const string InternetSettings = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";

    // Windows API imports for notifying system of proxy changes
    [DllImport("wininet.dll", SetLastError = true)]
    private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);

    private const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
    private const int INTERNET_OPTION_REFRESH = 37;

    /// <summary>
    /// Enable system proxy
    /// </summary>
    public static bool EnableProxy(string host = "127.0.0.1", int port = 8888)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(InternetSettings, writable: true);
            if (key == null)
            {
                Console.WriteLine("✗ Failed to open registry key");
                return false;
            }

            // Enable proxy
            key.SetValue("ProxyEnable", 1, RegistryValueKind.DWord);

            // Set proxy server
            var proxyServer = $"{host}:{port}";
            key.SetValue("ProxyServer", proxyServer, RegistryValueKind.String);

            // Notify system of changes
            NotifySystem();

            Console.WriteLine($"✓ System proxy enabled: {proxyServer}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Failed to enable proxy: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Disable system proxy
    /// </summary>
    public static bool DisableProxy()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(InternetSettings, writable: true);
            if (key == null)
            {
                Console.WriteLine("✗ Failed to open registry key");
                return false;
            }

            // Disable proxy
            key.SetValue("ProxyEnable", 0, RegistryValueKind.DWord);

            // Notify system of changes
            NotifySystem();

            Console.WriteLine("✓ System proxy disabled");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Failed to disable proxy: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Check if system proxy is currently enabled
    /// </summary>
    public static bool IsProxyEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(InternetSettings, writable: false);
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

    /// <summary>
    /// Notify Windows that proxy settings changed
    /// </summary>
    private static void NotifySystem()
    {
        try
        {
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
        }
        catch
        {
            // Ignore errors
        }
    }
}
