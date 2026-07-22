using ErrorOr;
using FinTrack.Modules.Users.Features.Register;
using FinTrack.Tests;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Tests.Modules.Users.Tests;

public class RegisterHandlerTests
{
    [Fact] // xUnit: marks a single test case
    public async Task Handle_NewEmail_CreatesUserAndReturnsResult()
    {
        // Arrange
        var ctx = TestHelpers.CreateContext(); // EF Core InMemory: isolated DB per test via TestHelpers
        var handler = new RegisterHandler(ctx);

        // Act
        var result = await handler.Handle(
            new RegisterCommand("test@fintrack.com", "pass1234", "Test User"), default
        );

        // Assert
        result.IsError.Should().BeFalse(); // FluentAssertions: readable assertion
        result.Value.Email.Should().Be("test@fintrack.com"); // FluentAssertions

        var user = await ctx.Users.SingleAsync(); // EF Core InMemory: query real DbContext
        user.Email.Should().Be("test@fintrack.com"); // FluentAssertions
        user.PasswordHash.Should().NotBe("pass1234"); // FluentAssertions
        user.PasswordHash.Should().StartWith("$2"); // FluentAssertions
    }

    [Fact] // xUnit: single test case
    public async Task Handle_NewEmail_NormalizesEmailToLowerCase()
    {
        var ctx = TestHelpers.CreateContext(); // EF Core InMemory: isolated DB per test
        var handler = new RegisterHandler(ctx);

        await handler.Handle(new RegisterCommand("TEST@FINTRACK.COM", "pass1234", "Test User"), default);

        var user = await ctx.Users.SingleAsync(); // EF Core InMemory
        user.Email.Should().Be("test@fintrack.com"); // FluentAssertions
    }

    [Fact] // xUnit: single test case
    public async Task Handle_DuplicateEmail_ReturnsConflictError()
    {
        var ctx = TestHelpers.CreateContext(); // EF Core InMemory: isolated DB per test
        var handler = new RegisterHandler(ctx);

        var command = new RegisterCommand("fintrack@gmail.com", "pass1234", "Test User");
        await handler.Handle(command, default);
        var result = await handler.Handle(command, default);

        result.IsError.Should().BeTrue(); // FluentAssertions
        result.FirstError.Type.Should().Be(ErrorType.Conflict); // FluentAssertions
        result.FirstError.Code.Should().Be("User.EmailAlreadyExists"); // FluentAssertions
    }

    [Fact] // xUnit: single test case
    public async Task Handle_DuplicateEmail_CaseInsensitive_ReturnsConflict()
    {
        var ctx = TestHelpers.CreateContext(); // EF Core InMemory: isolated DB per test
        var handler = new RegisterHandler(ctx);

        await handler.Handle(
            new RegisterCommand("test@fintrack.com", "password123", "User One"),
            default);

        // Same email in uppercase should be treated as duplicate
        var result = await handler.Handle(
            new RegisterCommand("TEST@FINTRACK.COM", "password456", "User Two"),
            default);

        result.IsError.Should().BeTrue(); // FluentAssertions
        result.FirstError.Type.Should().Be(ErrorType.Conflict); // FluentAssertions
    }
}