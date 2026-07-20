using PetShop.Api.Authentication;
using PetShop.Api.Diagnostics;
using PetShop.Api.Infrastructure.Persistence;
using PetShop.Observability.AspNetCore.Extensions;
using PetShop.Observability.Context;
using PetShop.Observability.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPetShopPersistence(builder.Configuration);
builder.Services.AddPetShopObservabilityPropagation();
builder.Services.AddPetShopApiSecurity(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseAuthentication();
app.UsePetShopObservabilityContext();
app.UseAuthorization();

app.MapHealthChecks("/health").WithName("Health");
app.MapGet(
        "/diagnostics",
        (IHostEnvironment environment, IExecutionContextAccessor executionContextAccessor) =>
            Results.Ok(DiagnosticsResponseFactory.Create(environment, executionContextAccessor)))
    .WithName("Diagnostics")
    .RequireAuthorization(PetShopAuthorizationPolicies.DiagnosticsAccess);

app.Run();
