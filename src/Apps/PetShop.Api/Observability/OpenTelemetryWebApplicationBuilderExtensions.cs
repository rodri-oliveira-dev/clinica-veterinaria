using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace PetShop.Api.Observability;

internal static class OpenTelemetryWebApplicationBuilderExtensions
{
    public const string ServiceName = "PetShop.Api";
    public const string ActivitySourceName = ServiceName;

    public static WebApplicationBuilder AddPetShopOpenTelemetry(
        this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        string? serviceVersion = typeof(ApiAssemblyMarker).Assembly.GetName().Version?.ToString();
        ResourceBuilder resource = ResourceBuilder
            .CreateDefault()
            .AddService(
                serviceName: ServiceName,
                serviceVersion: serviceVersion);

        Uri? otlpEndpoint = ResolveOtlpEndpoint(builder.Configuration);

        builder.Logging.AddOpenTelemetry(
            logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
                logging.SetResourceBuilder(resource);

                if (otlpEndpoint is not null)
                {
                    logging.AddOtlpExporter(options => options.Endpoint = otlpEndpoint);
                }
            });

        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resourceBuilder => resourceBuilder.AddService(
                serviceName: ServiceName,
                serviceVersion: serviceVersion))
            .WithTracing(
                tracing =>
                {
                    tracing
                        .AddSource(ActivitySourceName)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation();

                    if (otlpEndpoint is not null)
                    {
                        tracing.AddOtlpExporter(options => options.Endpoint = otlpEndpoint);
                    }
                })
            .WithMetrics(
                metrics =>
                {
                    metrics
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation();

                    if (otlpEndpoint is not null)
                    {
                        metrics.AddOtlpExporter(options => options.Endpoint = otlpEndpoint);
                    }
                });

        return builder;
    }

    private static Uri? ResolveOtlpEndpoint(ConfigurationManager configuration)
    {
        string? endpoint =
            configuration["OpenTelemetry:Otlp:Endpoint"] ??
            configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ??
            Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");

        return Uri.TryCreate(endpoint, UriKind.Absolute, out Uri? uri)
            ? uri
            : null;
    }
}
