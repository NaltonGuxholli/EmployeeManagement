using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Domain.Entities;

/// <summary>A unit of work belonging to a project, assignable to a project member.</summary>
public class TaskItem
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    /// <summary>The employee this task is assigned to. Null means unassigned.</summary>
    public int? AssignedToId { get; set; }
    public ApplicationUser? AssignedTo { get; set; }

    [Required]
    public int CreatedById { get; set; }
    public ApplicationUser CreatedBy { get; set; } = null!;

    [Required]
    public EmployeeTaskStatus Status { get; set; } = EmployeeTaskStatus.Open;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DueDate { get; set; }

    public DateTime? CompletedAt { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }
}
