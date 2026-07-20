using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using PetShop.Tutores.Domain;

namespace PetShop.Tutores.Infrastructure;

public static class ModuloTutoresPersistenceExtensions
{
    public static ModelBuilder ConfigurePersistenciaDoModuloTutores(
        this ModelBuilder modelBuilder,
        Expression<Func<Guid?>> tenantIdAtual)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        ArgumentNullException.ThrowIfNull(tenantIdAtual);

        modelBuilder.ApplyConfiguration(new TutorEntityTypeConfiguration(tenantIdAtual));
        modelBuilder.ApplyConfiguration(new AnimalEntityTypeConfiguration(tenantIdAtual));

        return modelBuilder;
    }

    public static void ValidarAlteracoesTenantOwnedDoModuloTutores(
        this ChangeTracker changeTracker,
        Guid? tenantIdAtual)
    {
        ArgumentNullException.ThrowIfNull(changeTracker);

        EntityEntry<Tutor>[] alteracoesDeTutores = ObterAlteracoes<Tutor>(changeTracker);
        EntityEntry<Animal>[] alteracoesDeAnimais = ObterAlteracoes<Animal>(changeTracker);

        if (alteracoesDeTutores.Length == 0 && alteracoesDeAnimais.Length == 0)
        {
            return;
        }

        if (!tenantIdAtual.HasValue)
        {
            throw new InvalidOperationException(
                "O tenant autenticado deve estar resolvido antes de alterar tutores ou animais.");
        }

        foreach (EntityEntry<Tutor> alteracao in alteracoesDeTutores)
        {
            if (alteracao.Entity.TenantId.Valor != tenantIdAtual.Value)
            {
                throw new InvalidOperationException(
                    "O tutor alterado deve pertencer ao tenant autenticado.");
            }
        }

        foreach (EntityEntry<Animal> alteracao in alteracoesDeAnimais)
        {
            if (alteracao.Entity.TenantId.Valor != tenantIdAtual.Value)
            {
                throw new InvalidOperationException(
                    "O animal alterado deve pertencer ao tenant autenticado.");
            }
        }
    }

    private static EntityEntry<T>[] ObterAlteracoes<T>(ChangeTracker changeTracker)
        where T : class =>
        changeTracker
            .Entries<T>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToArray();
}
