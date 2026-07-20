using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using PetShop.Tutores.Application;
using PetShop.Tutores.Infrastructure;

namespace PetShop.Tutores;

public static class ModuloTutoresServiceCollectionExtensions
{
    public static IServiceCollection AddModuloTutores<TDbContext>(
        this IServiceCollection services,
        Func<IServiceProvider, Guid?> obterTenantAtual,
        Func<IServiceProvider, string?>? obterSubjectAtual = null)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(obterTenantAtual);

        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<DbContext>(provider => provider.GetRequiredService<TDbContext>());
        services.AddScoped<IContextoTenantAtual>(provider =>
            new ContextoTenantAtualResolvido(obterTenantAtual(provider)));
        services.AddScoped<IContextoUsuarioAtual>(provider =>
            new ContextoUsuarioAtualResolvido(obterSubjectAtual?.Invoke(provider)));
        services.AddScoped<ITutoresRepository, TutoresRepository>();
        services.AddScoped<IAnimaisRepository, AnimaisRepository>();
        services.AddScoped<TutoresApplicationService>();
        services.AddScoped<AnimaisApplicationService>();

        return services;
    }

    private sealed class ContextoTenantAtualResolvido : IContextoTenantAtual
    {
        public ContextoTenantAtualResolvido(Guid? tenantId)
        {
            TenantId = tenantId;
        }

        public Guid? TenantId { get; }
    }

    private sealed class ContextoUsuarioAtualResolvido : IContextoUsuarioAtual
    {
        public ContextoUsuarioAtualResolvido(string? subject)
        {
            Subject = subject;
        }

        public string? Subject { get; }
    }
}
