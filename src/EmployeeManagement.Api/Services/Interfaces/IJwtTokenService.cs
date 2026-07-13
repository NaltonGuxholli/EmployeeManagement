using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Api.Services.Interfaces;

public interface IJwtTokenService
{
    /// <summary>Generates a signed JWT for the given user and their roles.</summary>
    (string token, DateTime expiresAtUtc) GenerateToken(ApplicationUser user, IList<string> roles);
}
