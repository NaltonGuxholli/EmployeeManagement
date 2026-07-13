using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Api.Models.DTOs.User;

/// <summary>Used by the logged-in employee to update their own profile data (no role/active fields).</summary>
public class UpdateProfileDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Position { get; set; }

    [MaxLength(150)]
    public string? Department { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
}
