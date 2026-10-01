using System.IdentityModel.Tokens.Jwt;
using CompanyManagement.Api.DTOs;
using CompanyManagement.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace CompanyManagement.Tests;

public class AuthTests
{
    [Fact]
    public async Task Register_assigns_User_role()
    {
        // Arrange
        var users = CreateUserManager();
        users.Setup(x => x.FindByEmailAsync("new@company.com")).ReturnsAsync((IdentityUser?)null);
        users.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), "User123!"))
            .ReturnsAsync(IdentityResult.Success);
        users.Setup(x => x.AddToRoleAsync(It.IsAny<IdentityUser>(), "User"))
            .ReturnsAsync(IdentityResult.Success);
        var service = new AuthService(users.Object, CreateConfiguration());

        // Act
        var result = await service.Register(new RegisterDto { Email = "new@company.com", Password = "User123!" });

        // Assert
        Assert.True(result.Item1);
        users.Verify(x => x.CreateAsync(It.Is<IdentityUser>(u => u.Email == "new@company.com"), "User123!"), Times.Once);
        users.Verify(x => x.AddToRoleAsync(It.IsAny<IdentityUser>(), "User"), Times.Once);
    }

    [Fact]
    public async Task Register_duplicate_email_returns_error()
    {
        // Arrange
        var users = CreateUserManager();
        users.Setup(x => x.FindByEmailAsync("existing@company.com"))
            .ReturnsAsync(new IdentityUser { Email = "existing@company.com" });
        var service = new AuthService(users.Object, CreateConfiguration());

        // Act
        var result = await service.Register(new RegisterDto { Email = "existing@company.com", Password = "User123!" });

        // Assert
        Assert.False(result.Item1);
        Assert.NotNull(result.Item2);
        users.Verify(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Login_invalid_credentials_returns_error_for_unknown_user()
    {
        // Arrange
        var users = CreateUserManager();
        users.Setup(x => x.FindByEmailAsync("missing@company.com")).ReturnsAsync((IdentityUser?)null);
        var service = new AuthService(users.Object, CreateConfiguration());

        // Act
        var result = await service.Login(new LoginDto { Email = "missing@company.com", Password = "User123!" });

        // Assert
        Assert.Null(result.Item1);
        Assert.Null(result.Item2);
        Assert.NotNull(result.Item3);
    }

    [Fact]
    public async Task Login_valid_returns_token_with_role_claim()
    {
        // Arrange
        var user = new IdentityUser { Id = "user-1", Email = "admin@company.com" };
        var users = CreateUserManager();
        users.Setup(x => x.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        users.Setup(x => x.CheckPasswordAsync(user, "Admin123!")).ReturnsAsync(true);
        users.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(["Admin"]);
        var service = new AuthService(users.Object, CreateConfiguration());

        // Act
        var result = await service.Login(new LoginDto { Email = user.Email!, Password = "Admin123!" });

        // Assert
        Assert.NotNull(result.Item1);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Item1);
        Assert.Contains(jwt.Claims, claim => claim.Type.EndsWith("/role") && claim.Value == "Admin");
    }

    private static Mock<UserManager<IdentityUser>> CreateUserManager() => new(
        Mock.Of<IUserStore<IdentityUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);

    private static IConfiguration CreateConfiguration() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "unit-test-development-key-that-is-long-enough-2026!",
            ["Jwt:Issuer"] = "CompanyManagement.Tests",
            ["Jwt:Audience"] = "CompanyManagement.Tests.Client",
            ["Jwt:ExpirationMinutes"] = "60"
        })
        .Build();
}
