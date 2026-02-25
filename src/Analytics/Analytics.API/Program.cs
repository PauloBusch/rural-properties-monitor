using Analytics.API.Middlewares;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;
using Serilog;
using Analitycs.IOC;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = "Analytics:";
});

var ingressUrl = builder.Configuration["Services:IngressApi"]
    ?? throw new InvalidOperationException("IngressApi URL not configured");

var redisConnection = builder.Configuration["Redis:ConnectionString"]
    ?? throw new InvalidOperationException("Redis connection not configured");

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddUrlGroup(
        new Uri(ingressUrl),
        name: "IngressApi",
        failureStatus: HealthStatus.Unhealthy,
        timeout: TimeSpan.FromSeconds(5))
    .AddRedis(
        redisConnection,
        name: "redis",
        failureStatus: HealthStatus.Unhealthy);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseHttpsRedirection();

app.UseHttpMetrics();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapMetrics();

app.MapControllers();

app.Run();

public partial class Program { }