using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.Events;
using AssignmentService.Application.Interfaces.MessageBrokers;

namespace AssignmentService.Infrastructure.MessageBrokers;

public class RabbitMqConnectionProvider : IRabbitMqConnectionProvider, IDisposable
{
    private IConnection? _connection = null;
    private readonly IConfiguration _config;
    private readonly ILogger<RabbitMqConnectionProvider> _logger;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private bool _disposed = false;
    
    public RabbitMqConnectionProvider(
        IConfiguration configuration,
        ILogger<RabbitMqConnectionProvider> logger)
    {
        _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IConnection> GetConnection()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Check if connection exists and is open
        if (_connection != null && _connection.IsOpen)
        {
            return _connection;
        }

        await _connectionLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            if (_connection != null && _connection.IsOpen)
            {
                return _connection;
            }

            // Close existing connection if it exists but is not open
            if (_connection != null)
            {
                await CloseConnectionSafely(_connection);
                _connection = null;
            }

            // Create new connection with retry logic
            _connection = await CreateConnectionWithRetry();
            
            _logger.LogInformation("Successfully connected to RabbitMQ at {Host}:{Port}", 
                _config["RabbitMQ:Host"], _config["RabbitMQ:Port"]);
            
            return _connection;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task CloseConnectionSafely(IConnection connection)
    {
        try
        {
            // Unregister event handlers first to avoid callback during close
            connection.ConnectionShutdownAsync -= OnConnectionShutdown;
            connection.ConnectionBlockedAsync -= OnConnectionBlocked;
            connection.ConnectionUnblockedAsync -= OnConnectionUnblocked;
            
            if (connection.IsOpen)
            {
                await connection.CloseAsync();
            }
            connection.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error closing existing RabbitMQ connection");
        }
    }

    private async Task<IConnection> CreateConnectionWithRetry(int maxRetries = 3, int delayMs = 2000)
    {
        // Validate configuration values
        var hostName = _config["RabbitMQ:Host"];
        var portStr = _config["RabbitMQ:Port"];
        
        if (string.IsNullOrWhiteSpace(hostName))
        {
            throw new InvalidOperationException("RabbitMQ:Host configuration is missing or empty");
        }

        if (!int.TryParse(portStr, out int port))
        {
            _logger.LogWarning("Invalid RabbitMQ:Port configuration '{Port}', using default port 5672", portStr);
            port = 5672;
        }

        var factory = new ConnectionFactory
        {
            HostName = hostName,
            Port = port,
            UserName = _config["RabbitMQ:Username"] ?? "guest",
            Password = _config["RabbitMQ:Password"] ?? "guest",
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            RequestedHeartbeat = TimeSpan.FromSeconds(60),
            RequestedConnectionTimeout = TimeSpan.FromSeconds(30)
        };

        Exception? lastException = null;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                _logger.LogInformation(
                    "Attempting to connect to RabbitMQ at {Host}:{Port} (attempt {Attempt}/{MaxRetries})", 
                    hostName, port, i + 1, maxRetries);
                
                var connection = await factory.CreateConnectionAsync();
                
                // Verify connection is actually open
                if (!connection.IsOpen)
                {
                    throw new InvalidOperationException("Connection was created but is not open");
                }
                
                // Register event handlers for connection recovery
                connection.ConnectionShutdownAsync += OnConnectionShutdown;
                connection.ConnectionBlockedAsync += OnConnectionBlocked;
                connection.ConnectionUnblockedAsync += OnConnectionUnblocked;
                
                _logger.LogInformation("Successfully established connection to RabbitMQ");
                
                return connection;
            }
            catch (BrokerUnreachableException ex)
            {
                lastException = ex;
                _logger.LogWarning(
                    "RabbitMQ broker is unreachable at {Host}:{Port} (attempt {Attempt}/{MaxRetries}). Retrying in {Delay}ms...", 
                    hostName, port, i + 1, maxRetries, delayMs);
                
                if (i < maxRetries - 1)
                {
                    await Task.Delay(delayMs);
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogError(ex, 
                    "Unexpected error connecting to RabbitMQ at {Host}:{Port} (attempt {Attempt}/{MaxRetries})", 
                    hostName, port, i + 1, maxRetries);
                
                if (i < maxRetries - 1)
                {
                    await Task.Delay(delayMs);
                }
            }
        }

        _logger.LogError(lastException, 
            "Failed to connect to RabbitMQ at {Host}:{Port} after {MaxRetries} attempts", 
            hostName, port, maxRetries);
        
        throw new InvalidOperationException(
            $"Unable to connect to RabbitMQ at {hostName}:{port} after {maxRetries} attempts. " +
            "Please ensure RabbitMQ server is running and accessible.", 
            lastException);
    }

    private Task OnConnectionShutdown(object sender, ShutdownEventArgs args)
    {
        if (args.Initiator == ShutdownInitiator.Application)
        {
            _logger.LogInformation("RabbitMQ connection closed by application: {Reason}", args.ReplyText);
        }
        else
        {
            _logger.LogWarning("RabbitMQ connection shutdown unexpectedly. Initiator: {Initiator}, Reason: {Reason}", 
                args.Initiator, args.ReplyText);
        }
        return Task.CompletedTask;
    }

    private Task OnConnectionBlocked(object sender, ConnectionBlockedEventArgs args)
    {
        _logger.LogWarning("RabbitMQ connection blocked: {Reason}", args.Reason);
        return Task.CompletedTask;
    }

    private Task OnConnectionUnblocked(object sender, AsyncEventArgs args)
    {
        _logger.LogInformation("RabbitMQ connection unblocked");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // Wait for lock with timeout to prevent hanging
            if (_connectionLock.Wait(TimeSpan.FromSeconds(5)))
            {
                try
                {
                    if (_connection != null)
                    {
                        try
                        {
                            // Unregister event handlers
                            _connection.ConnectionShutdownAsync -= OnConnectionShutdown;
                            _connection.ConnectionBlockedAsync -= OnConnectionBlocked;
                            _connection.ConnectionUnblockedAsync -= OnConnectionUnblocked;
                            
                            if (_connection.IsOpen)
                            {
                                _connection.CloseAsync().GetAwaiter().GetResult();
                            }
                            _connection.Dispose();
                            
                            _logger.LogInformation("RabbitMQ connection disposed successfully");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error disposing RabbitMQ connection");
                        }
                        finally
                        {
                            _connection = null;
                        }
                    }
                }
                finally
                {
                    _connectionLock.Release();
                }
            }
            else
            {
                _logger.LogWarning("Timeout waiting for connection lock during dispose");
            }
            
            _connectionLock.Dispose();
        }

        _disposed = true;
    }
}