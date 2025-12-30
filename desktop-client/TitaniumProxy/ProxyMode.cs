namespace TitaniumProxy;

/// <summary>
/// Proxy operation mode
/// </summary>
public enum ProxyMode
{
    /// <summary>
    /// Detect mode - monitor and log AI requests (default)
    /// </summary>
    Detect,

    /// <summary>
    /// Block mode - block AI requests except whitelisted hosts
    /// </summary>
    Block
}
