namespace PetShop.Api.Tenancy;

internal sealed class TenantContext : ITenantContext, ITenantContextSetter
{
    private TenantId? _tenantId;

    public bool IsResolved => _tenantId.HasValue;

    public TenantId TenantId => _tenantId ??
        throw new InvalidOperationException("Tenant context has not been resolved for this execution scope.");

    public void SetTenant(TenantId tenantId)
    {
        if (_tenantId.HasValue)
        {
            throw new InvalidOperationException("Tenant context was already resolved for this execution scope.");
        }

        _tenantId = tenantId;
    }
}
