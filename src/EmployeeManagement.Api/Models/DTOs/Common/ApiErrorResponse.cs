namespace EmployeeManagement.Api.Models.DTOs.Common;

/// <summary>Consistent error envelope returned by the global exception middleware.</summary>
public class ApiErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string TraceId { get; set; } = string.Empty;
}
