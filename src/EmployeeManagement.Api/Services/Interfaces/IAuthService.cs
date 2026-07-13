using EmployeeManagement.Api.Models.DTOs.Auth;

namespace EmployeeManagement.Api.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
}
