using ErrorOr;
using FinTrack.Common.Auth;
using FinTrack.Modules.Users.Entities;
using FinTrack.Modules.Users.Features.Login;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace FinTrack.Tests.Modules.Users.Tests;

public class LoginHandlerTests
{
    private static readonly IConfiguration Config = new ConfigurationManager() // IConfiguration: real config for JWT secret (no mock)
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "JwtSecret", "b0bc0eb5994bd19a71271ca678d5f6339dea6152a76c0006403c31b829f64147" }
        })
        .Build();

    private static readonly IJwtTokenService JwtService = new JwtTokenService(Config); // Real service (not mocked) - tests integration with JWT lib

    [Fact] // xUnit: single test case
    public async Task Handle_ValidCredentials_ReturnsToken()
    {
        var ctx = TestHelpers.CreateContext(); // EF Core InMemory: isolated DB per test
        await ctx.Users.AddAsync(new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "test@fintrack.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            FullName = "Test User",
            CreatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var handler = new LoginHandler(ctx, JwtService);
        var result = await handler.Handle(
            new LoginCommand("test@fintrack.com", "password123"), default);

        result.IsError.Should().BeFalse(); // FluentAssertions: readable assertions
        result.Value.Token.Should().NotBeNullOrWhiteSpace(); // FluentAssertions
        result.Value.Email.Should().Be("test@fintrack.com"); // FluentAssertions
    }

    [Fact] // xUnit: single test case
    public async Task Handle_WrongPassword_ReturnsUnauthorized()
    {
        var ctx = TestHelpers.CreateContext(); // EF Core InMemory: isolated DB per test
        await ctx.Users.AddAsync(new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "test@fintrack.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
            FullName = "Test User",
            CreatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var handler = new LoginHandler(ctx, JwtService);
        var result = await handler.Handle(
            new LoginCommand("test@fintrack.com", "wrongpassword"), default);

        result.IsError.Should().BeTrue(); // FluentAssertions
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized); // FluentAssertions
    }

    [Fact] // xUnit: single test case
    public async Task Handle_NonExistentEmail_ReturnsUnauthorized()
    {
        var ctx = TestHelpers.CreateContext(); // EF Core InMemory: isolated DB per test
        var handler = new LoginHandler(ctx, JwtService);

        var result = await handler.Handle(
            new LoginCommand("notexist@fintrack.com", "password123"), default);

        result.IsError.Should().BeTrue(); // FluentAssertions
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized); // FluentAssertions
        result.FirstError.Description.Should().Be("Invalid email or password"); // FluentAssertions
    }
}