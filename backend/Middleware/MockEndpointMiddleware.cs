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

        // Only process paths starting with /mock/
        if (!path.StartsWith("/mock/", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var sw = Stopwatch.StartNew();

        // Try to find a matching mock API that is enabled
        var mockApi = await mockApiService.FindMockAsync(method, path);

        sw.Stop();
        var responseTimeMs = sw.ElapsedMilliseconds;

        context.Response.ContentType = "application/json; charset=utf-8";

        if (mockApi == null)
        {
            // Check if a disabled mock exists
            var disabledApi = await mockApiService.FindDisabledMockAsync(method, path);

            if (disabledApi != null)
            {
                context.Response.StatusCode = 403;
                var disabledResult = JsonSerializer.Serialize(new
                {
                    code = 403,
                    message = "Mock API is disabled"
                });
                await context.Response.WriteAsync(disabledResult, Encoding.UTF8);

                await logService.LogRequestAsync(disabledApi.Id, method, path, 403, responseTimeMs);
                return;
            }

            context.Response.StatusCode = 404;
            var notFoundResult = JsonSerializer.Serialize(new
            {
                code = 404,
                message = "Mock API not found"
            });
            await context.Response.WriteAsync(notFoundResult, Encoding.UTF8);

            await logService.LogRequestAsync(null, method, path, 404, responseTimeMs);
            return;
        }

        context.Response.StatusCode = mockApi.StatusCode;
        await context.Response.WriteAsync(mockApi.ResponseBody, Encoding.UTF8);

        await logService.LogRequestAsync(mockApi.Id, method, path, mockApi.StatusCode, responseTimeMs);
    }
}