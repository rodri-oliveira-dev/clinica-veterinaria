namespace PetShop.Api.Tenancy;

internal static class TenancyApplicationBuilderExtensions
{
    public static IApplicationBuilder UsePetShopTenantContext(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseMiddleware<HttpTenantContextMiddleware>();
    }
}
