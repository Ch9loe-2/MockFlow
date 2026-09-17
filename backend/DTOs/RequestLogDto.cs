namespace MockFlowBackend.DTOs;

public class RequestLogDto
{
    public int Id { get; set; }
    public int? MockApiId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public long ResponseTimeMs { get; set; }
    public DateTime RequestedAt { get; set; }
}

public class DashboardStatsDto
{
    public int TotalApis { get; set; }
    public int EnabledApis { get; set; }
    public int TotalRequests { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public List<RequestLogDto> RecentLogs { get; set; } = new();
    public List<DailyRequestCountDto> DailyRequests { get; set; } = new();
}

public class DailyRequestCountDto
{
    public string Date { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class LogFilterDto
{
    public string? Method { get; set; }
    public int? StatusCode { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}