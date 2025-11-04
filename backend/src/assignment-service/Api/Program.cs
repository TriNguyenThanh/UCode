using Microsoft.EntityFrameworkCore;
using AssignmentService.Infrastructure.EF;
using AssignmentService.Application.Interfaces.Repositories;
using AssignmentService.Infrastructure.Repositories;
using AssignmentService.Infrastructure.Services;
using AssignmentService.Application.Interfaces.Services;
using AssignmentService.Api.Controllers;
using Microsoft.AspNetCore.Authorization.Policy;
using AssignmentService.Application.Mappings;
using AssignmentService.Api.Middlewares;
using Microsoft.AspNetCore.Mvc;
using AssignmentService.Application.DTOs.Common;
using Scrutor;
using AssignmentService.Api.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never; //dòng này Híu sửa, merge thì hãy giữ nhé
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Tắt Problem Details để dùng custom validation response
        // options.SuppressModelStateInvalidFilter = true;
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            
            return new BadRequestObjectResult(
                ApiResponse<object>.ErrorResponse(string.Join("; ", errors))
            );
        };
    });


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Problem Service API",
        Version = "v1",
        Description = "API for managing problems, assignments, and datasets in the ucode.io.vn platform",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Problem Service Team",
            Email = "support@ucode.io.vn"
        },
        License = new Microsoft.OpenApi.Models.OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Add custom schema filter for examples
    c.SchemaFilter<ExampleSchemaFilter>();
    
    // Add operation filter for error responses
    c.OperationFilter<ErrorResponseOperationFilter>();

    // Add JWT Authentication
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Add DbContext với Snake Case Naming Convention
builder.Services.AddDbContext<AssignmentDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("AssignmentDb")
        ?? builder.Configuration["ConnectionStrings:AssignmentDb"]
        ?? builder.Configuration["AssignmentDb"]
        ?? throw new InvalidOperationException("Connection string 'AssignmentDb' not found.");
    
    options.UseSqlServer(
        connectionString,
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    );

    // =====Enable Snake Case Naming =====
    options.UseSnakeCaseNamingConvention();
});

// Register HttpClient for UserService
builder.Services.AddHttpClient<IUserServiceClient, UserServiceClient>();

// Register RabbitMQ Connection Provider as Singleton (connection pooling)
builder.Services.AddSingleton<AssignmentService.Application.Interfaces.MessageBrokers.IRabbitMqConnectionProvider, AssignmentService.Infrastructure.MessageBrokers.RabbitMqConnectionProvider>();

// ===== DEPENDENCY INJECTION =====
// Tự động đăng ký các service và repository
var assemblies = AppDomain.CurrentDomain.GetAssemblies();

builder.Services.Scan(scan => scan
    .FromAssemblies(assemblies)
    .AddClasses(classes => classes.InNamespaces(
        "AssignmentService.Infrastructure.Services",
        "AssignmentService.Infrastructure.Repositories",
        "AssignmentService.Infrastructure.MessageBrokers"))
    .AsImplementedInterfaces()
    .WithScopedLifetime());

// Register Repositories
// builder.Services.AddScoped<IProblemRepository, ProblemRepository>();
// builder.Services.AddScoped<ILanguageRepository, LanguageRepository>();

// Register Services
// builder.Services.AddScoped<IProblemService, ProblemService>();
// builder.Services.AddScoped<ILanguageService, LanguageService>();

//Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AssignmentDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("🔄 Checking database connection...");
        
        // Tạo database nếu chưa tồn tại và chạy migrations
        await context.Database.MigrateAsync();
        logger.LogInformation("✅ Database migrated successfully!");
        
        // Seed data nếu database trống
        await AssignmentDbContextSeed.SeedAsync(context);
        logger.LogInformation("✅ Database seeded successfully!");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ An error occurred while migrating or seeding the database.");
        
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<GatewayRoleMiddleware>();
app.UseAuthorization();
app.MapControllers();


app.Run();
