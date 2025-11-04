using Amazon.S3;
using file_service.Services;
using file_service.Middlewares;
using file_service.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add AWS S3 Service
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddScoped<IS3Service, S3Service>();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<S3HealthCheck>("s3_health_check");

// Add Controllers
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
            ?? new[] { "http://localhost:3000", "http://localhost:5173" };
        
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "File Service API", 
        Version = "v1",
        Description = "API for managing files using AWS S3"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "File Service API v1");
    });
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

app.MapGet("/", () => new
{
    service = "File Service",
    version = "2.0",
    status = "Running",
    features = new[]
    {
        "Category-based file upload with validation",
        "Magic bytes verification for images",
        "Filename sanitization",
        "MIME type validation",
        "File size limits per category"
    },
    endpoints = new[]
    {
        "GET /api/files/categories - Get all file categories",
        "GET /api/files/categories/{id} - Get specific category config",
        "POST /api/files/upload - Upload a file (requires category parameter)",
        "GET /api/files/download/{key} - Download a file",
        "DELETE /api/files/{key} - Delete a file",
        "POST /api/files/presigned-url - Get presigned URL",
        "GET /api/files/list - List files",
        "GET /api/files/exists/{key} - Check if file exists"
    },
    categories = new
    {
        AssignmentDocument = 1,
        CodeSubmission = 2,
        Image = 3,
        Avatar = 4,
        TestCase = 5,
        Reference = 6,
        Document = 7
    }
}).WithName("ServiceInfo").WithOpenApi();

app.Run();

