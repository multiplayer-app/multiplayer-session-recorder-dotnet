using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace WebApiOpenApi.Controllers;

[ApiController]
[Route("session-recorder-demo")]
public class SessionRecorderDemoController(ILogger<SessionRecorderDemoController> _logger) : ControllerBase
{
    /// <summary>
    /// Demonstrates HTTP request/response capture with simple text
    /// </summary>
    [HttpPost("simple")]
    public async Task<IActionResult> SimplePost([FromBody] SimpleRequest request)
    {
        _logger.LogInformation("Processing simple request for user: {UserId}", request.UserId);
        
        await Task.Delay(100); // Simulate some processing
        
        var response = new SimpleResponse
        {
            Message = $"Hello {request.Name}!",
            ProcessedAt = DateTime.UtcNow,
            UserId = request.UserId
        };
        
        return Ok(response);
    }
    
    /// <summary>
    /// Demonstrates HTTP request/response capture with sensitive data (will be masked)
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Processing login request for user: {Username}", request.Username);
        
        await Task.Delay(200); // Simulate authentication
        
        // This response contains sensitive data that should be masked
        var response = new LoginResponse
        {
            Success = true,
            Token = "jwt-token-12345-secret-key",
            User = new UserInfo
            {
                Id = 123,
                Username = request.Username,
                Email = $"{request.Username}@example.com"
            },
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
        
        return Ok(response);
    }
    
    /// <summary>
    /// Demonstrates HTTP request capture with large payload
    /// </summary>
    [HttpPost("large-data")]
    public async Task<IActionResult> ProcessLargeData([FromBody] LargeDataRequest request)
    {
        _logger.LogInformation("Processing large data request with {ItemCount} items", request.Items?.Count ?? 0);
        
        await Task.Delay(300); // Simulate processing
        
        var response = new LargeDataResponse
        {
            ProcessedCount = request.Items?.Count ?? 0,
            ProcessedAt = DateTime.UtcNow,
            Summary = $"Processed {request.Items?.Count ?? 0} items successfully"
        };
        
        return Ok(response);
    }
    
    /// <summary>
    /// Demonstrates error handling with middleware capture
    /// </summary>
    [HttpPost("error")]
    public IActionResult SimulateError([FromBody] ErrorRequest request)
    {
        _logger.LogWarning("Simulating error for type: {ErrorType}", request.ErrorType);
        
        return request.ErrorType?.ToLower() switch
        {
            "badrequest" => BadRequest(new { Error = "Invalid request data", Code = "BAD_REQUEST" }),
            "unauthorized" => Unauthorized(new { Error = "Authentication required", Code = "UNAUTHORIZED" }),
            "notfound" => NotFound(new { Error = "Resource not found", Code = "NOT_FOUND" }),
            "internal" => StatusCode(500, new { Error = "Internal server error", Code = "INTERNAL_ERROR" }),
            _ => Ok(new { Message = "No error simulated", RequestType = request.ErrorType })
        };
    }
}

// Request/Response DTOs
public class SimpleRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public int UserId { get; set; }
}

public class SimpleResponse
{
    public string Message { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
    public int UserId { get; set; }
}

public class LoginRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty; // This will be masked
}

public class LoginResponse
{
    public bool Success { get; set; }
    public string Token { get; set; } = string.Empty; // This contains "secret" and will be masked
    public UserInfo User { get; set; } = new();
    public DateTime ExpiresAt { get; set; }
}

public class UserInfo
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class LargeDataRequest
{
    public List<DataItem>? Items { get; set; }
}

public class DataItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}

public class LargeDataResponse
{
    public int ProcessedCount { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string Summary { get; set; } = string.Empty;
}

public class ErrorRequest
{
    public string? ErrorType { get; set; }
}
