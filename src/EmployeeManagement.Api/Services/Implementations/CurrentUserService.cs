using System.Security.Claims;
using EmployeeManagement.Api.Services.Interfaces;
using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Api.Services.Implementations;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    public bool IsAdministrator =>
        _httpContextAccessor.HttpContext?.User?.IsInRole(UserRoles.Administrator) ?? false;

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
