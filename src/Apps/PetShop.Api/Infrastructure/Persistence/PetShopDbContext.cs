using Microsoft.EntityFrameworkCore;

using PetShop.Api.Tenancy;
using PetShop.Tutores.Infrastructure;

namespace PetShop.Api.Infrastructure.Persistence;

public sealed class PetShopDbContext : DbContext
{
    private readonly ITenantContext? _tenantContext;

    public PetShopDbContext(DbContextOptions<PetShopDbContext> options)
        : this(options, tenantContext: null)
    {
    }

    public PetShopDbContext(
        DbContextOptions<PetShopDbContext> options,
        ITenantContext? tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public Guid? TenantIdAtual =>
        _tenantContext is { IsResolved: true } ? _tenantContext.TenantId.Value : null;

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ChangeTracker.ValidarAlteracoesTenantOwnedDoModuloTutores(TenantIdAtual);

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ChangeTracker.ValidarAlteracoesTenantOwnedDoModuloTutores(TenantIdAtual);

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ConfigurePersistenciaDoModuloTutores(() => TenantIdAtual);
    }
}
