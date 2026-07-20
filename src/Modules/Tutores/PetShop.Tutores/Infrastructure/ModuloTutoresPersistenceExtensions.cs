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

        return modelBuilder;
    }

    public static void ValidarAlteracoesTenantOwnedDoModuloTutores(
        this ChangeTracker changeTracker,
        Guid? tenantIdAtual)
    {
        ArgumentNullException.ThrowIfNull(changeTracker);

        EntityEntry<Tutor>[] alteracoes = changeTracker
            .Entries<Tutor>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToArray();

        if (alteracoes.Length == 0)
        {
            return;
        }

        if (!tenantIdAtual.HasValue)
        {
            throw new InvalidOperationException(
                "O tenant autenticado deve estar resolvido antes de alterar tutores.");
        }

        foreach (EntityEntry<Tutor> alteracao in alteracoes)
        {
            if (alteracao.Entity.TenantId.Valor != tenantIdAtual.Value)
            {
                throw new InvalidOperationException(
                    "O tutor alterado deve pertencer ao tenant autenticado.");
            }
        }
    }
}
