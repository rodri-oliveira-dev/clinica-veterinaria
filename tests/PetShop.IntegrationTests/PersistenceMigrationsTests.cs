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
        await using (PetShopDbContext resetContext = _postgresql.CreateDbContext())
        {
            await resetContext.Database.EnsureDeletedAsync(TestContext.Current.CancellationToken);
        }

        await using PetShopDbContext dbContext = _postgresql.CreateDbContext();

        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);

        IEnumerable<string> pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(
            TestContext.Current.CancellationToken);
        IEnumerable<string> appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync(
            TestContext.Current.CancellationToken);

        Assert.Empty(pendingMigrations);
        string[] migrationNames = appliedMigrations.ToArray();

        Assert.Equal(5, migrationNames.Length);
        Assert.Contains(migrationNames, migration => migration.Contains("InitialPersistenceFoundation", StringComparison.Ordinal));
        Assert.Contains(migrationNames, migration => migration.Contains("AddTutoresPersistence", StringComparison.Ordinal));
        Assert.Contains(migrationNames, migration => migration.Contains("AddAnimaisPersistence", StringComparison.Ordinal));
        Assert.Contains(migrationNames, migration => migration.Contains("AddAnimalResponsibilityTransfers", StringComparison.Ordinal));
        Assert.Contains(migrationNames, migration => migration.Contains("AddAnimalDeathLifecycle", StringComparison.Ordinal));
    }
}
