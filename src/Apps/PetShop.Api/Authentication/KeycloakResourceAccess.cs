using System.Security.Claims;
using System.Text.Json;

namespace PetShop.Api.Authentication;

internal static class KeycloakResourceAccess
{
    private const string ResourceAccessClaimType = "resource_access";

    public static bool HasClientRole(
        ClaimsPrincipal principal,
        string clientId,
        string role)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(role);

        foreach (Claim claim in principal.FindAll(ResourceAccessClaimType))
        {
            if (ContainsRole(claim.Value, clientId, role))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsRole(
        string resourceAccess,
        string clientId,
        string role)
    {
        if (string.IsNullOrWhiteSpace(resourceAccess))
        {
            return false;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(resourceAccess);

            if (!document.RootElement.TryGetProperty(clientId, out JsonElement clientAccess) ||
                !clientAccess.TryGetProperty("roles", out JsonElement roles) ||
                roles.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            return roles
                .EnumerateArray()
                .Any(element => element.ValueKind == JsonValueKind.String && element.GetString() == role);
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
