namespace MockFlowBackend.DTOs;

public class MockApiDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string ResponseBody { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateMockApiDto
{
    public string Name { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; } = 200;
    public string ResponseBody { get; set; } = "{}";
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
}

public class UpdateMockApiDto
{
    public string Name { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; } = 200;
    public string ResponseBody { get; set; } = "{}";
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
}