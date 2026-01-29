using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace TitaniumProxy;

/// <summary>
/// Manage certificate installation
/// </summary>
public static class CertificateManager
{
    private const string CertificateName = "Titanium Root Certificate Authority";
    
    /// <summary>
    /// Get the certificate path in user's home directory
    /// </summary>
    public static string GetCertPath()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var certDir = Path.Combine(home, ".titaniumproxy");
        Directory.CreateDirectory(certDir);
        return Path.Combine(certDir, "titanium-ca-cert.pfx");
    }

    /// <summary>
    /// Check if certificate is installed in the Trusted Root store
    /// </summary>
    public static bool IsInstalled()
    {
        try
        {
            using var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);

            foreach (var cert in store.Certificates)
            {
                if (cert.Subject.Contains("Titanium") || cert.FriendlyName.Contains("Titanium"))
                {
                    return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Install certificate silently to Trusted Root store
    /// </summary>
    public static bool InstallSilent(X509Certificate2 certificate)
    {
        try
        {
            // Remove old certificate first (if exists)
            UninstallAll();

            // Open the Trusted Root store for current user
            using var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);

            // Set a friendly name for easy identification
            certificate.FriendlyName = CertificateName;

            // Add the certificate
            store.Add(certificate);

            Console.WriteLine("✓ Certificate installed successfully");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Failed to install certificate: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Uninstall all Titanium certificates from Trusted Root store
    /// </summary>
    public static bool UninstallAll()
    {
        try
        {
            using var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);

            var certsToRemove = new List<X509Certificate2>();

            foreach (var cert in store.Certificates)
            {
                if (cert.Subject.Contains("Titanium") || cert.FriendlyName.Contains("Titanium"))
                {
                    certsToRemove.Add(cert);
                }
            }

            foreach (var cert in certsToRemove)
            {
                store.Remove(cert);
            }

            if (certsToRemove.Count > 0)
            {
                Console.WriteLine($"✓ Removed {certsToRemove.Count} certificate(s)");
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Failed to uninstall certificates: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Export certificate to file for backup
    /// </summary>
    public static bool ExportToFile(X509Certificate2 certificate, string filePath)
    {
        try
        {
            var certData = certificate.Export(X509ContentType.Pfx);
            File.WriteAllBytes(filePath, certData);
            Console.WriteLine($"✓ Certificate exported to: {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Failed to export certificate: {ex.Message}");
            return false;
        }
    }
}
