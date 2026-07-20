using PetShop.Api.Authentication;
using PetShop.Api.Diagnostics;
using PetShop.Api.Infrastructure.Persistence;
using PetShop.Api.Observability;
using PetShop.Api.Tenancy;
using PetShop.Observability.AspNetCore.Extensions;
using PetShop.Observability.Context;
using PetShop.Observability.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPetShopPersistence(builder.Configuration);
builder.Services.AddPetShopObservabilityPropagation();
builder.AddPetShopOpenTelemetry();
builder.Services.AddPetShopApiSecurity(builder.Configuration, builder.Environment);
builder.Services.AddPetShopTenantContext();

var app = builder.Build();

app.UseAuthentication();
app.UsePetShopTenantContext();
app.UsePetShopObservabilityContext();
app.UseAuthorization();

app.MapHealthChecks("/health").WithName("Health");
app.MapGet(
        "/diagnostics",
        (
            IHostEnvironment environment,
            IExecutionContextAccessor executionContextAccessor,
            ITenantContext tenantContext) =>
            Results.Ok(DiagnosticsResponseFactory.Create(environment, executionContextAccessor, tenantContext)))
    .WithName("Diagnostics")
    .RequireAuthorization(PetShopAuthorizationPolicies.DiagnosticsAccess);

app.Run();
