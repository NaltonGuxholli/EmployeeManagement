using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace EmployeeManagement.Api.Data.Seed;

/// <summary>
/// Ensures the two application roles exist and creates a default
/// Administrator account on first run so there is a way to log in
/// (the spec explicitly states there is no self-registration endpoint).
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var roleName in new[] { UserRoles.Administrator, UserRoles.Employee })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole(roleName));
            }
        }

        var adminEmail = config["SeedAdmin:Email"] ?? "admin@company.com";
        var adminPassword = config["SeedAdmin:Password"] ?? "Admin@12345";

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin is null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
                Position = "Administrator",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, UserRoles.Administrator);
            }
        }
    }
}
