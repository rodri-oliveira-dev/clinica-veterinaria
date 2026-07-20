using Microsoft.EntityFrameworkCore;

namespace PetShop.Api.Infrastructure.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    public const string ConnectionStringName = "petshop";

    public static IServiceCollection AddPetShopPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<PetShopDbContext>(
            (serviceProvider, options) => ConfigurePostgreSql(
                options,
                GetConnectionString(serviceProvider.GetRequiredService<IConfiguration>())));

        services.AddHealthChecks()
            .AddDbContextCheck<PetShopDbContext>("postgresql");

        return services;
    }

    public static void ConfigurePostgreSql(
        DbContextOptionsBuilder options,
        string connectionString)
    {
        options
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history"))
            .UseSnakeCaseNamingConvention();
    }

    private static string GetConnectionString(IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString(ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is required to configure PostgreSQL persistence.");
        }

        return connectionString;
    }
}
