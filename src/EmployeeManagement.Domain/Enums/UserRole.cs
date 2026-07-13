namespace EmployeeManagement.Domain.Enums;

/// <summary>
/// Application role names. Kept as constants so they can be reused with
/// ASP.NET Core Identity's role based authorization ([Authorize(Roles = ...)]).
/// </summary>
public static class UserRoles
{
    public const string Administrator = "Administrator";
    public const string Employee = "Employee";
}
