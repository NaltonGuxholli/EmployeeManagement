using Microsoft.AspNetCore.Identity;

namespace EmployeeManagement.Domain.Entities;

/// <summary>Identity role with an int key, matching <see cref="ApplicationUser"/>.</summary>
public class ApplicationRole : IdentityRole<int>
{
    public ApplicationRole() { }

    public ApplicationRole(string roleName) : base(roleName) { }
}
