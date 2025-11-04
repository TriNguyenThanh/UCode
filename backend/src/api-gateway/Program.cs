using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ApiGateway.Middlewares;

// Custom rate limiting implementation

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/api-gateway-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Configure to listen on port 5000
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!"))
        };
    });

// Add Rate Limiting (Custom implementation)
builder.Services.AddMemoryCache();

// Add Ocelot with multiple configuration files
// Thay thế phần load multiple files
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // app.UseSwaggerForOcelotUI();
}

app.UseSerilogRequestLogging();

// Add Security Headers - nên đặt đầu tiên
app.UseMiddleware<SecurityHeadersMiddleware>();

// Add Error Handling - nên đặt sớm để catch tất cả exceptions
app.UseMiddleware<ErrorHandlingMiddleware>();

// Add Request Logging
app.UseMiddleware<RequestLoggingMiddleware>();

// Add CORS middleware (có thể dùng custom hoặc built-in)
// app.UseMiddleware<CorsMiddleware>(); // Nếu muốn dùng custom
app.UseCors("AllowAll"); // Hoặc dùng built-in như hiện tại

// Request validation - trước khi xử lý authentication
app.UseMiddleware<RequestValidationMiddleware>();

// JWT Authentication middleware (custom) hoặc dùng built-in
// app.UseMiddleware<JwtAuthenticationMiddleware>(); // Nếu muốn dùng custom
app.UseAuthentication(); // Hoặc dùng built-in như hiện tại
app.UseAuthorization();

app.UseMiddleware<ClaimsToHeadersMiddleware>();

app.UseMiddleware<CustomRateLimitMiddleware>();


// Ocelot phải chạy cuối cùng để forward requests
await app.UseOcelot();

app.MapControllers();

app.Run();
