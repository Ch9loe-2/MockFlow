using System.Diagnostics;
using System.Text;
using System.Text.Json;
using MockFlowBackend.Services;

namespace MockFlowBackend.Middleware;

public class MockEndpointMiddleware
{
    private readonly RequestDelegate _next;

    public MockEndpointMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, MockApiService mockApiService, RequestLogService logService)
    {
        var path = context.Request.Path.Value ?? "";
        var method = context.Request.Method;

        if (!path.StartsWith("/mock/", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var sw = Stopwatch.StartNew();
        context.Response.ContentType = "application/json; charset=utf-8";

        var mockApi = await mockApiService.FindMockIgnoreEnabledAsync(method, path);

        if (mockApi == null)
        {
            context.Response.StatusCode = 404;
            var notFoundResult = JsonSerializer.Serialize(new { code = 404, message = "Mock API not found" });
            await context.Response.WriteAsync(notFoundResult, Encoding.UTF8);
            sw.Stop();
            await logService.LogRequestAsync(null, method, path, 404, sw.ElapsedMilliseconds);
            return;
        }

        if (!mockApi.IsEnabled)
        {
            context.Response.StatusCode = 403;
            var result = JsonSerializer.Serialize(new { code = 403, message = "Mock API is disabled" });
            await context.Response.WriteAsync(result, Encoding.UTF8);
            sw.Stop();
            await logService.LogRequestAsync(mockApi.Id, method, path, 403, sw.ElapsedMilliseconds);
            return;
        }

        context.Response.StatusCode = mockApi.StatusCode;
        await context.Response.WriteAsync(mockApi.ResponseBody, Encoding.UTF8);
        sw.Stop();

        await logService.LogRequestAsync(mockApi.Id, method, path, mockApi.StatusCode, sw.ElapsedMilliseconds);
    }
}