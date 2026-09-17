using Microsoft.EntityFrameworkCore;
using MockFlowBackend.Models;

namespace MockFlowBackend.Data;

public class MockFlowDbContext : DbContext
{
    public MockFlowDbContext(DbContextOptions<MockFlowDbContext> options) : base(options) { }

    public DbSet<MockApi> MockApis => Set<MockApi>();
    public DbSet<RequestLog> RequestLogs => Set<RequestLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MockApi>(entity =>
        {
            entity.HasIndex(e => new { e.Method, e.Path }).IsUnique();
            entity.Property(e => e.ResponseBody).HasColumnType("TEXT");
        });

        modelBuilder.Entity<RequestLog>(entity =>
        {
            entity.HasIndex(e => e.RequestedAt);
            entity.HasOne(e => e.MockApi)
                  .WithMany(m => m.RequestLogs)
                  .HasForeignKey(e => e.MockApiId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}