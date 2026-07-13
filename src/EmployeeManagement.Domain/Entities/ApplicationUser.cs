using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace EmployeeManagement.Domain.Entities;

/// <summary>
/// Extends ASP.NET Core Identity's user with the profile fields
/// required by the employee administration domain.
/// </summary>
public class ApplicationUser : IdentityUser<int>
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Position { get; set; }

    [MaxLength(150)]
    public string? Department { get; set; }

    [MaxLength(20)]
    public string? PhoneNumberSecondary { get; set; }

    /// <summary>Relative path (under wwwroot) to the uploaded profile picture.</summary>
    [MaxLength(500)]
    public string? ProfilePicturePath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<ProjectEmployee> ProjectMemberships { get; set; } = new List<ProjectEmployee>();
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    public ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();

    public string FullName => $"{FirstName} {LastName}".Trim();
}
