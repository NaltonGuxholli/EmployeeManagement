using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Api.Models.DTOs.User;

/// <summary>Used by an Administrator to update any user's account/profile.</summary>
public class UpdateUserDto
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

    public bool IsActive { get; set; } = true;

    /// <summary>Optional role reassignment, e.g. "Administrator" or "Employee".</summary>
    public string? Role { get; set; }
}
