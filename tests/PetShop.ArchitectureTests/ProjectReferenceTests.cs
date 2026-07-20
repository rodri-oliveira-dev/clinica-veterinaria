using System.Reflection;

using PetShop.Api;
using PetShop.Observability.AspNetCore.Extensions;
using PetShop.Observability.Context;

namespace PetShop.ArchitectureTests;

public sealed class ProjectReferenceTests
{
    [Fact]
    public void ObservabilityCore_DoesNotDependOnAspNetCore()
    {
        Assembly observability = typeof(IExecutionContextAccessor).Assembly;

        Assert.DoesNotContain(
            observability.GetReferencedAssemblies(),
            reference => reference.Name?.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal) == true);
    }

    [Fact]
    public void Api_UsesAspNetCoreObservabilityAdapter()
    {
        Assembly api = typeof(ApiAssemblyMarker).Assembly;

        Assert.Contains(
            api.GetReferencedAssemblies(),
            reference => reference.Name == typeof(ObservabilityApplicationBuilderExtensions).Assembly.GetName().Name);
    }

    [Fact]
    public void Api_DoesNotReferenceAppHost()
    {
        Assembly api = typeof(ApiAssemblyMarker).Assembly;

        Assert.DoesNotContain(
            api.GetReferencedAssemblies(),
            reference => reference.Name == "PetShop.AppHost");
    }
}
