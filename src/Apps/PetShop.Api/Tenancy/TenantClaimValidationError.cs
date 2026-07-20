namespace PetShop.Api.Tenancy;

public enum TenantClaimValidationError
{
    None,
    Missing,
    Multiple,
    Empty,
    Invalid
}
