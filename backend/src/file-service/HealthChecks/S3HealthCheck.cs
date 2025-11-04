using Microsoft.Extensions.Diagnostics.HealthChecks;
using Amazon.S3;
using Amazon.S3.Model;

namespace file_service.HealthChecks;

public class S3HealthCheck : IHealthCheck
{
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<S3HealthCheck> _logger;

    public S3HealthCheck(IAmazonS3 s3Client, IConfiguration configuration, ILogger<S3HealthCheck> logger)
    {
        _s3Client = s3Client;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var bucketName = _configuration["AWS:BucketName"];
            
            if (string.IsNullOrEmpty(bucketName))
            {
                return HealthCheckResult.Unhealthy("S3 bucket name not configured");
            }

            // Check if bucket exists and is accessible
            var request = new ListObjectsV2Request
            {
                BucketName = bucketName,
                MaxKeys = 1
            };

            await _s3Client.ListObjectsV2Async(request, cancellationToken);

            return HealthCheckResult.Healthy($"S3 bucket '{bucketName}' is accessible");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "S3 health check failed");
            return HealthCheckResult.Unhealthy("Cannot connect to S3", ex);
        }
    }
}
