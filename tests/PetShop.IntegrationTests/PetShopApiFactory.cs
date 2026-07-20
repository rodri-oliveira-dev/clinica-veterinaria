using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PetShop.Api.Infrastructure.Persistence;

namespace PetShop.IntegrationTests;

public sealed class PetShopApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    private readonly IReadOnlyDictionary<string, string?> _configuration;
    private readonly Action<IServiceCollection>? _configureTestServices;

    public PetShopApiFactory(
        string connectionString,
        IReadOnlyDictionary<string, string?>? configuration = null,
        Action<IServiceCollection>? configureTestServices = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        _connectionString = connectionString;
        _configuration = configuration ?? new Dictionary<string, string?>();
        _configureTestServices = configureTestServices;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureAppConfiguration(
            (_, configuration) =>
            {
                var values = new Dictionary<string, string?>
                {
                    [$"ConnectionStrings:{PersistenceServiceCollectionExtensions.ConnectionStringName}"] =
                        _connectionString,
                    ["Authentication:Authority"] = "http://localhost:8080/realms/petshop-local",
                    ["Authentication:Audience"] = "petshop-api",
                    ["Authentication:RoleClientId"] = "petshop-api",
                    ["Authentication:RequiredRole"] = "petshop.access",
                    ["Authentication:RequireHttpsMetadata"] = "false"
                };

                foreach (KeyValuePair<string, string?> value in _configuration)
                {
                    values[value.Key] = value.Value;
                }

                configuration.AddInMemoryCollection(values);
            });

        if (_configureTestServices is not null)
        {
            builder.ConfigureTestServices(_configureTestServices);
        }
    }
}
