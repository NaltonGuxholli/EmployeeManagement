using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Api.Models.DTOs.Task;

public class CreateTaskDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    public int ProjectId { get; set; }

    /// <summary>Optional. Must be a member of the project. Null = unassigned.</summary>
    public int? AssignedToId { get; set; }

    public DateTime? DueDate { get; set; }
}

public class UpdateTaskDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public int? AssignedToId { get; set; }

    public DateTime? DueDate { get; set; }

    [Required]
    public EmployeeTaskStatus Status { get; set; }
}

public class AssignTaskDto
{
    [Required]
    public int EmployeeId { get; set; }
}
