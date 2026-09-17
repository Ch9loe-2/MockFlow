namespace MockFlowBackend.Models;

public class MockApi
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; } = 200;
    public string ResponseBody { get; set; } = "{}";
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<RequestLog> RequestLogs { get; set; } = new();
}