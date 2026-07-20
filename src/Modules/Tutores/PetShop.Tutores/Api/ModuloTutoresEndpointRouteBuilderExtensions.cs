using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace PetShop.Tutores;

public static class ModuloTutoresEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapModuloTutores(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        _ = endpoints
            .MapGroup("/tutores")
            .WithTags("Tutores")
            .RequireAuthorization();

        return endpoints;
    }
}
