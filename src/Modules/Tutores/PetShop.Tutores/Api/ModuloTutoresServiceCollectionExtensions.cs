using Microsoft.Extensions.DependencyInjection;

namespace PetShop.Tutores;

public static class ModuloTutoresServiceCollectionExtensions
{
    public static IServiceCollection AddModuloTutores(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services;
    }
}
