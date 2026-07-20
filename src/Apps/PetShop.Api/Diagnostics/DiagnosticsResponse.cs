using PetShop.Api.Tenancy;
using PetShop.Observability.Context;

namespace PetShop.Api.Diagnostics;

public sealed record DiagnosticsResponse(
    string Service,
    string Environment,
    string TenantId,
    string? CorrelationId);

public static class DiagnosticsResponseFactory
{
    public static DiagnosticsResponse Create(
        IHostEnvironment environment,
        IExecutionContextAccessor executionContextAccessor,
        ITenantContext tenantContext)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(executionContextAccessor);
        ArgumentNullException.ThrowIfNull(tenantContext);

        return new DiagnosticsResponse(
            Service: "PetShop.Api",
            Environment: environment.EnvironmentName,
            TenantId: tenantContext.TenantId.ToString(),
            CorrelationId: executionContextAccessor.Current?.CorrelationId);
    }
}
