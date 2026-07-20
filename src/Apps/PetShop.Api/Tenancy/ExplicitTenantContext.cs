namespace PetShop.Api.Tenancy;

public sealed class ExplicitTenantContext : ITenantContext
{
    public ExplicitTenantContext(TenantId tenantId)
    {
        TenantId = tenantId;
    }

    public bool IsResolved => true;

    public TenantId TenantId { get; }
}
