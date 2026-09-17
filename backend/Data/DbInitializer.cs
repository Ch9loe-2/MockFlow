using MockFlowBackend.Models;

namespace MockFlowBackend.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(MockFlowDbContext context)
    {
        if (context.MockApis.Any()) return;

        var mockApis = new List<MockApi>
        {
            new()
            {
                Name = "Users List",
                Method = "GET",
                Path = "/mock/users",
                StatusCode = 200,
                ResponseBody = """
                {
                  "code": 200,
                  "message": "success",
                  "data": [
                    { "id": 1, "name": "Chloe", "email": "chloe@example.com" },
                    { "id": 2, "name": "Alice", "email": "alice@example.com" },
                    { "id": 3, "name": "Bob", "email": "bob@example.com" }
                  ]
                }
                """,
                Description = "Returns a list of users",
                IsEnabled = true
            },
            new()
            {
                Name = "Products List",
                Method = "GET",
                Path = "/mock/products",
                StatusCode = 200,
                ResponseBody = """
                {
                  "code": 200,
                  "message": "success",
                  "data": [
                    { "id": 1, "name": "Laptop", "price": 9999.00 },
                    { "id": 2, "name": "Mouse", "price": 99.00 },
                    { "id": 3, "name": "Keyboard", "price": 299.00 }
                  ]
                }
                """,
                Description = "Returns a list of products",
                IsEnabled = true
            },
            new()
            {
                Name = "User Login",
                Method = "POST",
                Path = "/mock/login",
                StatusCode = 200,
                ResponseBody = """
                {
                  "code": 200,
                  "message": "Login successful",
                  "data": {
                    "token": "mock-jwt-token-abc123",
                    "expiresIn": 3600
                  }
                }
                """,
                Description = "Mock login endpoint",
                IsEnabled = true
            },
            new()
            {
                Name = "System Status",
                Method = "GET",
                Path = "/mock/system/status",
                StatusCode = 200,
                ResponseBody = """
                {
                  "code": 200,
                  "message": "success",
                  "data": {
                    "status": "healthy",
                    "uptime": "99.9%",
                    "version": "1.0.0"
                  }
                }
                """,
                Description = "Returns system health status",
                IsEnabled = true
            }
        };

        context.MockApis.AddRange(mockApis);
        await context.SaveChangesAsync();
    }
}