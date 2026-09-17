using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MockFlowBackend.DTOs;
using MockFlowBackend.Services;

namespace MockFlowBackend.Controllers;

[ApiController]
[Route("api/mock-apis")]
public class MockApisController : ControllerBase
{
    private readonly MockApiService _service;

    public MockApisController(MockApiService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<MockApiDto>>> GetAll()
    {
        return await _service.GetAllAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MockApiDto>> GetById(int id)
    {
        var api = await _service.GetByIdAsync(id);
        if (api == null)
            return NotFound(new { code = 404, message = "Mock API not found" });
        return api;
    }

    [HttpPost]
    public async Task<ActionResult<MockApiDto>> Create([FromBody] CreateMockApiDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { code = 400, message = "Name is required" });
        if (dto.Name.Length > 100)
            return BadRequest(new { code = 400, message = "Name must be 100 characters or fewer" });
        if (string.IsNullOrWhiteSpace(dto.Path))
            return BadRequest(new { code = 400, message = "Path is required" });
        if (dto.Path.Length > 500)
            return BadRequest(new { code = 400, message = "Path must be 500 characters or fewer" });
        if (dto.Method.ToUpper() is not ("GET" or "POST" or "PUT" or "DELETE"))
            return BadRequest(new { code = 400, message = "Method must be GET, POST, PUT, or DELETE" });

        if (!IsValidJson(dto.ResponseBody))
            return BadRequest(new { code = 400, message = "Response JSON format error" });

        try
        {
            var api = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = api.Id }, api);
        }
        catch (Exception ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true)
        {
            return Conflict(new { code = 409, message = $"A mock API with method '{dto.Method}' and path '{dto.Path}' already exists" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MockApiDto>> Update(int id, [FromBody] UpdateMockApiDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { code = 400, message = "Name is required" });
        if (dto.Name.Length > 100)
            return BadRequest(new { code = 400, message = "Name must be 100 characters or fewer" });
        if (string.IsNullOrWhiteSpace(dto.Path))
            return BadRequest(new { code = 400, message = "Path is required" });
        if (dto.Path.Length > 500)
            return BadRequest(new { code = 400, message = "Path must be 500 characters or fewer" });
        if (dto.Method.ToUpper() is not ("GET" or "POST" or "PUT" or "DELETE"))
            return BadRequest(new { code = 400, message = "Method must be GET, POST, PUT, or DELETE" });

        if (!IsValidJson(dto.ResponseBody))
            return BadRequest(new { code = 400, message = "Response JSON format error" });

        try
        {
            var api = await _service.UpdateAsync(id, dto);
            if (api == null)
                return NotFound(new { code = 404, message = "Mock API not found" });

            return api;
        }
        catch (Exception ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true)
        {
            return Conflict(new { code = 409, message = $"A mock API with method '{dto.Method}' and path '{dto.Path}' already exists" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success)
            return NotFound(new { code = 404, message = "Mock API not found" });
        return NoContent();
    }

    private static bool IsValidJson(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        try
        {
            JsonDocument.Parse(value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}