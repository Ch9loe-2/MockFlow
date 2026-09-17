using Microsoft.EntityFrameworkCore;
using MockFlowBackend.Data;
using MockFlowBackend.Middleware;
using MockFlowBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Database
var dbPath = Path.Combine(AppContext.BaseDirectory, "mockflow.db");
builder.Services.AddDbContext<MockFlowDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Services
builder.Services.AddScoped<MockApiService>();
builder.Services.AddScoped<RequestLogService>();

// CORS for frontend
// ⚠️ Production: replace AllowAnyOrigin with WithOrigins("https://yourdomain.com")
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Create and seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MockFlowDbContext>();
    await context.Database.EnsureCreatedAsync();
    await DbInitializer.SeedAsync(context);
}

app.UseCors();
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<MockEndpointMiddleware>();
app.MapControllers();

await app.RunAsync();