namespace TitaniumProxy;

/// <summary>
/// Manages whitelist for block mode
/// </summary>
public class WhitelistManager
{
    private readonly HashSet<string> _whitelistedHosts;

    public WhitelistManager(IEnumerable<string>? whitelistedHosts = null)
    {
        _whitelistedHosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        if (whitelistedHosts != null)
        {
            foreach (var host in whitelistedHosts)
            {
                _whitelistedHosts.Add(host.Trim().ToLower());
            }
        }
    }

    /// <summary>
    /// Check if host is whitelisted
    /// </summary>
    public bool IsWhitelisted(string host)
    {
        var hostLower = host.ToLower();

        // Exact match
        if (_whitelistedHosts.Contains(hostLower))
            return true;

        // Wildcard match (e.g., *.example.com)
        foreach (var whitelisted in _whitelistedHosts)
        {
            if (whitelisted.StartsWith("*."))
            {
                var domain = whitelisted.Substring(2);
                if (hostLower.EndsWith(domain))
                    return true;
            }
            // Subdomain match (e.g., example.com matches api.example.com)
            else if (hostLower.EndsWith("." + whitelisted))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Add host to whitelist
    /// </summary>
    public void AddHost(string host)
    {
        _whitelistedHosts.Add(host.Trim().ToLower());
    }

    /// <summary>
    /// Remove host from whitelist
    /// </summary>
    public void RemoveHost(string host)
    {
        _whitelistedHosts.Remove(host.Trim().ToLower());
    }

    /// <summary>
    /// Get all whitelisted hosts
    /// </summary>
    public IReadOnlySet<string> GetWhitelistedHosts()
    {
        return _whitelistedHosts;
    }

    /// <summary>
    /// Load whitelist from file
    /// </summary>
    public static WhitelistManager LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            return new WhitelistManager();

        var hosts = File.ReadAllLines(filePath)
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith("#"))
            .Select(line => line.Trim());

        return new WhitelistManager(hosts);
    }

    /// <summary>
    /// Save whitelist to file
    /// </summary>
    public void SaveToFile(string filePath)
    {
        File.WriteAllLines(filePath, _whitelistedHosts.OrderBy(h => h));
    }
}
