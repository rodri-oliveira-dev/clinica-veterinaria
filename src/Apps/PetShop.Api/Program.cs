using Microsoft.AspNetCore.Diagnostics.HealthChecks;

using PetShop.Api.Authentication;
using PetShop.Api.Diagnostics;
using PetShop.Api.HttpApi;
using PetShop.Api.Infrastructure.Persistence;
using PetShop.Api.Observability;
using PetShop.Api.Tenancy;
using PetShop.Observability.AspNetCore.Extensions;
using PetShop.Observability.Context;
using PetShop.Observability.DependencyInjection;
using PetShop.Tutores;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPetShopHttpApiContracts();
builder.Services.AddPetShopPersistence(builder.Configuration);
builder.Services.AddPetShopObservabilityPropagation();
builder.AddPetShopOpenTelemetry();
builder.Services.AddPetShopApiSecurity(builder.Configuration, builder.Environment);
builder.Services.AddPetShopTenantContext();
builder.Services.AddModuloTutores<PetShopDbContext>(serviceProvider =>
{
    ITenantContext tenantContext = serviceProvider.GetRequiredService<ITenantContext>();

    return tenantContext.IsResolved ? tenantContext.TenantId.Value : null;
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseAuthentication();
app.UsePetShopObservabilityContext();
app.UsePetShopTenantContext();
app.UseStatusCodePages(ApiProblemDetailsWriter.WriteStatusCodeProblemAsync);
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
}

app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = registration => registration.Tags.Contains("live", StringComparer.Ordinal)
    })
    .WithName("Liveness")
    .AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = registration => registration.Tags.Contains("ready", StringComparer.Ordinal)
    })
    .WithName("Readiness")
    .AllowAnonymous();
app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = registration => registration.Tags.Contains("ready", StringComparer.Ordinal)
    })
    .WithName("Health")
    .AllowAnonymous();
app.MapGet(
        "/diagnostics",
        (
            IHostEnvironment environment,
            IExecutionContextAccessor executionContextAccessor,
            ITenantContext tenantContext) =>
            Results.Ok(DiagnosticsResponseFactory.Create(environment, executionContextAccessor, tenantContext)))
    .WithName("Diagnostics")
    .RequireAuthorization(PetShopAuthorizationPolicies.DiagnosticsAccess);
app.MapModuloTutores(PetShopAuthorizationPolicies.TutoresAccess);

app.Run();
