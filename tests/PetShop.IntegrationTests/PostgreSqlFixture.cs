using Microsoft.EntityFrameworkCore;

using PetShop.Api.Infrastructure.Persistence;

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
}
