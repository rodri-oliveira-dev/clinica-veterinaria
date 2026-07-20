using PetShop.Api.HttpApi;

namespace PetShop.Api.Tenancy;

internal sealed class HttpTenantContextMiddleware
{
    private readonly RequestDelegate _next;

    public HttpTenantContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantContextSetter tenantContext)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(tenantContext);

        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        TenantClaimValidationError error = TenantIdParser.TryGetTenantId(context.User, out TenantId tenantId);
        if (error != TenantClaimValidationError.None)
        {
            await RejectMissingTenantAsync(context);
            return;
        }

        tenantContext.SetTenant(tenantId);

        await _next(context);
    }

    private static async Task RejectMissingTenantAsync(HttpContext context)
    {
        await ApiProblemDetailsWriter.WriteAsync(
            context,
            StatusCodes.Status403Forbidden,
            "Authenticated tenant context is required.",
            "The authenticated principal must contain exactly one valid tenant_id claim.",
            ApiErrorCodes.TenantRequired);
    }
}
