using System.Security.Claims;

namespace PetShop.Api.Tenancy;

public static class TenantIdParser
{
    public const string ClaimType = "tenant_id";

    public static TenantClaimValidationError TryGetTenantId(
        ClaimsPrincipal principal,
        out TenantId tenantId)
    {
        ArgumentNullException.ThrowIfNull(principal);

        tenantId = default;
        Claim[] claims = principal.FindAll(ClaimType).ToArray();

        if (claims.Length == 0)
        {
            return TenantClaimValidationError.Missing;
        }

        if (claims.Length > 1)
        {
            return TenantClaimValidationError.Multiple;
        }

        string? value = claims[0].Value;
        if (string.IsNullOrWhiteSpace(value))
        {
            return TenantClaimValidationError.Empty;
        }

        return TenantId.TryParse(value, out tenantId)
            ? TenantClaimValidationError.None
            : TenantClaimValidationError.Invalid;
    }
}
