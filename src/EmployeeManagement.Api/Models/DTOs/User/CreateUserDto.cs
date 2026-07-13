using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Api.Models.DTOs.User;

/// <summary>Used by an Administrator to create a new user account.</summary>
public class CreateUserDto
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; set; } = string.Empty;

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

    /// <summary>Must be one of <see cref="UserRoles"/>. Defaults to Employee.</summary>
    [Required]
    public string Role { get; set; } = UserRoles.Employee;
}
