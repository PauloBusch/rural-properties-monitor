using System.Net;
using System.Text.Json;

namespace Analytics.API.Middlewares;
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var correlationId = context.TraceIdentifier;

            _logger.LogError(ex,
                "Unhandled exception occurred. CorrelationId: {CorrelationId}",
                correlationId);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                message = "An unexpected error occurred.",
                correlationId
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }
}
