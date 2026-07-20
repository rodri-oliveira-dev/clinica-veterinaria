using PetShop.Observability.Context;

namespace PetShop.Api.Diagnostics;

public sealed record DiagnosticsResponse(
    string Service,
    string Environment,
    string? CorrelationId);

public static class DiagnosticsResponseFactory
{
    public static DiagnosticsResponse Create(
        IHostEnvironment environment,
        IExecutionContextAccessor executionContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(executionContextAccessor);

        return new DiagnosticsResponse(
            Service: "PetShop.Api",
            Environment: environment.EnvironmentName,
            CorrelationId: executionContextAccessor.Current?.CorrelationId);
    }
}
