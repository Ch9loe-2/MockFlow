using Microsoft.EntityFrameworkCore;
using MockFlowBackend.Data;
using MockFlowBackend.DTOs;
using MockFlowBackend.Models;

namespace MockFlowBackend.Services;

public class RequestLogService
{
    private readonly MockFlowDbContext _context;

    public RequestLogService(MockFlowDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> GetDashboardAsync()
    {
        var totalApis = await _context.MockApis.CountAsync();
        var enabledApis = await _context.MockApis.CountAsync(a => a.IsEnabled);
        var totalRequests = await _context.RequestLogs.CountAsync();
        var avgResponseTime = totalRequests > 0
            ? await _context.RequestLogs.AverageAsync(l => l.ResponseTimeMs)
            : 0;

        var recentLogs = await _context.RequestLogs
            .OrderByDescending(l => l.RequestedAt)
            .Take(10)
            .ToListAsync();

        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        var recentLogsForChart = await _context.RequestLogs
            .Where(l => l.RequestedAt >= sevenDaysAgo)
            .Select(l => l.RequestedAt.Date)
            .ToListAsync();

        var dailyRequests = recentLogsForChart
            .GroupBy(d => d)
            .Select(g => new DailyRequestCountDto
            {
                Date = g.Key.ToString("MM-dd"),
                Count = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();

        return new DashboardStatsDto
        {
            TotalApis = totalApis,
            EnabledApis = enabledApis,
            TotalRequests = totalRequests,
            AverageResponseTimeMs = Math.Round(avgResponseTime, 1),
            RecentLogs = recentLogs.Select(MapToDto).ToList(),
            DailyRequests = dailyRequests
        };
    }

    public async Task<PagedResult<RequestLogDto>> GetLogsAsync(LogFilterDto filter)
    {
        var query = _context.RequestLogs.AsQueryable();

        if (!string.IsNullOrEmpty(filter.Method))
            query = query.Where(l => l.Method == filter.Method.ToUpper());

        if (filter.StatusCode.HasValue)
            query = query.Where(l => l.StatusCode == filter.StatusCode.Value);

        if (!string.IsNullOrEmpty(filter.StatusCodeRange))
        {
            var parts = filter.StatusCodeRange.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[0], out var min) && int.TryParse(parts[1], out var max))
                query = query.Where(l => l.StatusCode >= min && l.StatusCode <= max);
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(l => l.RequestedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<RequestLogDto>
        {
            Items = items.Select(MapToDto).ToList(),
            Total = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task LogRequestAsync(int? mockApiId, string method, string path, int statusCode, long responseTimeMs)
    {
        var log = new RequestLog
        {
            MockApiId = mockApiId,
            Method = method,
            Path = path,
            StatusCode = statusCode,
            ResponseTimeMs = responseTimeMs,
            RequestedAt = DateTime.UtcNow
        };

        _context.RequestLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ClearLogsAsync()
    {
        _context.RequestLogs.RemoveRange(_context.RequestLogs);
        await _context.SaveChangesAsync();
        return true;
    }

    private static RequestLogDto MapToDto(RequestLog log) => new()
    {
        Id = log.Id,
        MockApiId = log.MockApiId,
        Method = log.Method,
        Path = log.Path,
        StatusCode = log.StatusCode,
        ResponseTimeMs = log.ResponseTimeMs,
        RequestedAt = log.RequestedAt
    };
}