using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

using PetShop.Api.Infrastructure.Persistence;

namespace PetShop.IntegrationTests;

public sealed class PetShopApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public PetShopApiFactory(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureAppConfiguration(
            (_, configuration) => configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    [$"ConnectionStrings:{PersistenceServiceCollectionExtensions.ConnectionStringName}"] =
                        _connectionString,
                }));
    }
}
