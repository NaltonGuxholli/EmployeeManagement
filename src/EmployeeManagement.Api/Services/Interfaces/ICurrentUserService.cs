namespace EmployeeManagement.Api.Services.Interfaces;

/// <summary>Exposes the identity of the currently authenticated request, read from the JWT claims.</summary>
public interface ICurrentUserService
{
    int? UserId { get; }
    bool IsAdministrator { get; }
    bool IsAuthenticated { get; }
}
