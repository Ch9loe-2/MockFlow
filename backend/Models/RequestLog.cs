namespace MockFlowBackend.Models;

public class RequestLog
{
    public int Id { get; set; }
    public int? MockApiId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public long ResponseTimeMs { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public MockApi? MockApi { get; set; }
}