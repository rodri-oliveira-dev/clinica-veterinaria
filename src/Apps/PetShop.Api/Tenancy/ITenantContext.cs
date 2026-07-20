namespace PetShop.Api.Tenancy;

public interface ITenantContext
{
    bool IsResolved { get; }

    TenantId TenantId { get; }
}
