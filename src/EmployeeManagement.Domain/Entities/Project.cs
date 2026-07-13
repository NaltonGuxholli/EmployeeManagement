using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Domain.Entities;

/// <summary>A project that groups tasks and has a set of assigned employees.</summary>
public class Project
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DueDate { get; set; }

    /// <summary>Soft indicator, true while the project has not been archived/closed.</summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<ProjectEmployee> ProjectEmployees { get; set; } = new List<ProjectEmployee>();
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
