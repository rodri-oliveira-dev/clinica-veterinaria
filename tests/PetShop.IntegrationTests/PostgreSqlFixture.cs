using Microsoft.EntityFrameworkCore;

using PetShop.Api.Infrastructure.Persistence;
using PetShop.Api.Tenancy;

using Testcontainers.PostgreSql;

namespace PetShop.IntegrationTests;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("petshop")
        .WithUsername("petshop")
        .WithPassword("petshop")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public PetShopDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PetShopDbContext>();
        PersistenceServiceCollectionExtensions.ConfigurePostgreSql(options, ConnectionString);

        return new PetShopDbContext(options.Options);
    }

    public PetShopDbContext CreateDbContext(Guid tenantId)
    {
        var options = new DbContextOptionsBuilder<PetShopDbContext>();
        PersistenceServiceCollectionExtensions.ConfigurePostgreSql(options, ConnectionString);

        return new PetShopDbContext(options.Options, new ExplicitTenantContext(new TenantId(tenantId)));
    }

    public async Task ResetDatabaseAsync()
    {
        await using PetShopDbContext dbContext = CreateDbContext();

        await dbContext.Database.EnsureDeletedAsync(TestContext.Current.CancellationToken);
        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);
    }
}
