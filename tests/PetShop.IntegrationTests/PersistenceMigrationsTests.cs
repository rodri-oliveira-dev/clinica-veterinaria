using Microsoft.EntityFrameworkCore;

using PetShop.Api.Infrastructure.Persistence;

namespace PetShop.IntegrationTests;

public sealed class PersistenceMigrationsTests : IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture _postgresql;

    public PersistenceMigrationsTests(PostgreSqlFixture postgresql)
    {
        _postgresql = postgresql;
    }

    [Fact]
    public async Task Migrations_InitializeEmptyPostgreSqlDatabase()
    {
        await using PetShopDbContext dbContext = _postgresql.CreateDbContext();

        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);

        IEnumerable<string> pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(
            TestContext.Current.CancellationToken);
        IEnumerable<string> appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync(
            TestContext.Current.CancellationToken);

        Assert.Empty(pendingMigrations);
        Assert.Single(appliedMigrations);
        Assert.Contains("InitialPersistenceFoundation", appliedMigrations.Single(), StringComparison.Ordinal);
    }
}
