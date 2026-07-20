using PetShop.Api.Diagnostics;
using PetShop.Observability.AspNetCore.Extensions;
using PetShop.Observability.Context;
using PetShop.Observability.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddPetShopObservabilityPropagation();

var app = builder.Build();

app.UsePetShopObservabilityContext();

app.MapHealthChecks("/health").WithName("Health");
app.MapGet(
        "/diagnostics",
        (IHostEnvironment environment, IExecutionContextAccessor executionContextAccessor) =>
            Results.Ok(DiagnosticsResponseFactory.Create(environment, executionContextAccessor)))
    .WithName("Diagnostics");

app.Run();
