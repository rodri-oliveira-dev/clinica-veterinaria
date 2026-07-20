using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders;

using PetShop.Api.Diagnostics;
using PetShop.Observability.Context;
using PetShop.Observability.Propagation;

namespace PetShop.UnitTests;

public sealed class DiagnosticsResponseFactoryTests
{
    [Fact]
    public void Create_UsesCurrentCorrelationContext()
    {
        var environment = new TestHostEnvironment
        {
            EnvironmentName = Environments.Development
        };
        var accessor = new ExecutionContextAccessor();
        var snapshot = new PropagationContextSnapshot(
            CorrelationId: "96db35bb-f512-410d-8737-f029cbf00bf8",
            TenantId: null,
            TraceParent: null,
            TraceState: null,
            Baggage: null);

        using IDisposable _ = accessor.Push(snapshot);

        DiagnosticsResponse response = DiagnosticsResponseFactory.Create(environment, accessor);

        Assert.Equal("PetShop.Api", response.Service);
        Assert.Equal(Environments.Development, response.Environment);
        Assert.Equal(snapshot.CorrelationId, response.CorrelationId);
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;

        public string ApplicationName { get; set; } = "PetShop.Api";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
