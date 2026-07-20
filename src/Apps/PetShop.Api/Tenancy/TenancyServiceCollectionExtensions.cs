using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PetShop.Api.Tenancy;

internal static class TenancyServiceCollectionExtensions
{
    public static IServiceCollection AddPetShopTenantContext(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<TenantContext>();
        services.TryAddScoped<ITenantContext>(provider => provider.GetRequiredService<TenantContext>());
        services.TryAddScoped<ITenantContextSetter>(provider => provider.GetRequiredService<TenantContext>());

        return services;
    }
}
