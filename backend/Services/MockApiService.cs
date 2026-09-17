using Microsoft.EntityFrameworkCore;
using MockFlowBackend.Data;
using MockFlowBackend.DTOs;
using MockFlowBackend.Models;

namespace MockFlowBackend.Services;

public class MockApiService
{
    private readonly MockFlowDbContext _context;

    public MockApiService(MockFlowDbContext context)
    {
        _context = context;
    }

    public async Task<List<MockApiDto>> GetAllAsync()
    {
        var apis = await _context.MockApis
            .OrderByDescending(a => a.UpdatedAt)
            .ToListAsync();

        return apis.Select(MapToDto).ToList();
    }

    public async Task<MockApiDto?> GetByIdAsync(int id)
    {
        var api = await _context.MockApis.FindAsync(id);
        return api == null ? null : MapToDto(api);
    }

    public async Task<MockApiDto> CreateAsync(CreateMockApiDto dto)
    {
        var api = new MockApi
        {
            Name = dto.Name,
            Method = dto.Method.ToUpper(),
            Path = dto.Path,
            StatusCode = dto.StatusCode,
            ResponseBody = dto.ResponseBody,
            Description = dto.Description,
            IsEnabled = dto.IsEnabled,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MockApis.Add(api);
        await _context.SaveChangesAsync();

        return MapToDto(api);
    }

    public async Task<MockApiDto?> UpdateAsync(int id, UpdateMockApiDto dto)
    {
        var api = await _context.MockApis.FindAsync(id);
        if (api == null) return null;

        api.Name = dto.Name;
        api.Method = dto.Method.ToUpper();
        api.Path = dto.Path;
        api.StatusCode = dto.StatusCode;
        api.ResponseBody = dto.ResponseBody;
        api.Description = dto.Description;
        api.IsEnabled = dto.IsEnabled;
        api.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDto(api);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var api = await _context.MockApis.FindAsync(id);
        if (api == null) return false;

        _context.MockApis.Remove(api);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<MockApi?> FindMockAsync(string method, string path)
    {
        return await _context.MockApis
            .FirstOrDefaultAsync(a => a.Method == method.ToUpper()
                                   && a.Path == path
                                   && a.IsEnabled);
    }

    public async Task<MockApi?> FindDisabledMockAsync(string method, string path)
    {
        return await _context.MockApis
            .FirstOrDefaultAsync(a => a.Method == method.ToUpper()
                                   && a.Path == path
                                   && !a.IsEnabled);
    }

    private static MockApiDto MapToDto(MockApi api) => new()
    {
        Id = api.Id,
        Name = api.Name,
        Method = api.Method,
        Path = api.Path,
        StatusCode = api.StatusCode,
        ResponseBody = api.ResponseBody,
        Description = api.Description,
        IsEnabled = api.IsEnabled,
        CreatedAt = api.CreatedAt,
        UpdatedAt = api.UpdatedAt
    };
}