using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Api.Models.DTOs.User;

public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, MinLength(6), MaxLength(100)]
    public string NewPassword { get; set; } = string.Empty;
}
