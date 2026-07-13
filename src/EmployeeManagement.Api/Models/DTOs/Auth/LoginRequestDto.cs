using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Api.Models.DTOs.Auth;

/// <summary>Payload for POST /api/auth/login.</summary>
public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
