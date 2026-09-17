using Microsoft.AspNetCore.Mvc;
using MockFlowBackend.DTOs;
using MockFlowBackend.Services;

namespace MockFlowBackend.Controllers;

[ApiController]
[Route("api/logs")]
public class LogsController : ControllerBase
{
    private readonly RequestLogService _service;

    public LogsController(RequestLogService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<RequestLogDto>>> GetLogs(
        [FromQuery] string? method,
        [FromQuery] int? statusCode,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);

        var filter = new LogFilterDto
        {
            Method = method,
            StatusCode = statusCode,
            Page = page,
            PageSize = pageSize
        };

        return await _service.GetLogsAsync(filter);
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboard()
    {
        return await _service.GetDashboardAsync();
    }

    [HttpDelete]
    public async Task<ActionResult> ClearLogs()
    {
        await _service.ClearLogsAsync();
        return NoContent();
    }
}