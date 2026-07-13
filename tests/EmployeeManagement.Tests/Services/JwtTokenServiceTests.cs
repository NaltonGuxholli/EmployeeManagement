using EmployeeManagement.Api.Services.Implementations;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EmployeeManagement.Tests.Services;

public class JwtTokenServiceTests
{
    private static JwtTokenService CreateSut() => new(Options.Create(new JwtOptions
    {
        Key = "unit-test-super-secret-signing-key-1234567890",
        Issuer = "TestIssuer",
        Audience = "TestAudience",
        ExpiryMinutes = 60
    }));

    [Fact]
    public void GenerateToken_ShouldReturnNonEmptyToken_WithFutureExpiry()
    {
        var sut = CreateSut();
        var user = new ApplicationUser
        {
            Id = 1,
            Email = "jane@test.com",
            UserName = "jane@test.com",
            FirstName = "Jane",
            LastName = "Doe"
        };

        var (token, expiresAtUtc) = sut.GenerateToken(user, new List<string> { UserRoles.Employee });

        token.Should().NotBeNullOrWhiteSpace();
        expiresAtUtc.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void GenerateToken_ShouldEmbedRoleClaim()
    {
        var sut = CreateSut();
        var user = new ApplicationUser
        {
            Id = 2,
            Email = "admin@test.com",
            UserName = "admin@test.com",
            FirstName = "Admin",
            LastName = "User"
        };

        var (token, _) = sut.GenerateToken(user, new List<string> { UserRoles.Administrator });

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == UserRoles.Administrator);
    }
}
