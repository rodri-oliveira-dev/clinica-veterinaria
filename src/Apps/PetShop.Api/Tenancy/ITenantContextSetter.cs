namespace PetShop.Api.Tenancy;

internal interface ITenantContextSetter
{
    void SetTenant(TenantId tenantId);
}
