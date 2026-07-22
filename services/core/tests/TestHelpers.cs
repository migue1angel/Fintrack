using FinTrack.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Tests;

public static class TestHelpers
{
    // Creates a NEW isolated InMemory database per test call
    // - Guid.NewGuid() ensures unique DB name = no shared state between tests
    // - InMemory provider: fast (<1ms), no external dependency, supports EF Core LINQ
    // - Trade-off: doesn't test PostgreSQL-specific behavior (indexes, constraints, types)
    //   Use Testcontainers (PostgreSQL in Docker) for integration tests in CI
    public static ApplicationDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
}