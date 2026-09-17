using System.Net;
using System.Text.Json;

namespace MockFlowBackend.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception processing request: {Path}", context.Request.Path);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json; charset=utf-8";

            var errorBody = new Dictionary<string, object>
            {
                ["code"] = 500,
                ["message"] = "Internal server error"
            };

            // In development, include exception details for debugging
            if (_env.IsDevelopment())
            {
                errorBody["detail"] = ex.Message;
                errorBody["stackTrace"] = ex.StackTrace ?? "";
            }

            var result = JsonSerializer.Serialize(errorBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            await context.Response.WriteAsync(result);
        }
    }
}