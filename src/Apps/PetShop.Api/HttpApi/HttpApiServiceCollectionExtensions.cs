using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

using PetShop.Observability.Propagation;

namespace PetShop.Api.HttpApi;

internal static class HttpApiServiceCollectionExtensions
{
    public static IServiceCollection AddPetShopHttpApiContracts(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                int statusCode = context.ProblemDetails.Status ?? context.HttpContext.Response.StatusCode;
                context.ProblemDetails.Status = statusCode;
                context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;
                if (string.IsNullOrWhiteSpace(context.ProblemDetails.Type) ||
                    string.Equals(context.ProblemDetails.Type, "about:blank", StringComparison.Ordinal))
                {
                    context.ProblemDetails.Type = $"urn:petshop:error:{ResolveCode(statusCode)}";
                }

                context.ProblemDetails.Extensions.TryAdd("code", ResolveCode(statusCode));
                context.ProblemDetails.Extensions.TryAdd(
                    "correlationId",
                    ApiProblemDetails.ResolveCorrelationId(context.HttpContext));
            };
        });

        services.AddExceptionHandler<UnhandledExceptionHandler>();

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
            options.SerializerOptions.RespectRequiredConstructorParameters = true;
        });

        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            options.AddOperationTransformer<StandardHttpOperationTransformer>();
        });

        return services;
    }

    private static string ResolveCode(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => ApiErrorCodes.InvalidRequest,
            StatusCodes.Status401Unauthorized => ApiErrorCodes.Unauthenticated,
            StatusCodes.Status403Forbidden => ApiErrorCodes.Forbidden,
            StatusCodes.Status404NotFound => ApiErrorCodes.NotFound,
            StatusCodes.Status409Conflict => ApiErrorCodes.Conflict,
            _ => ApiErrorCodes.Unexpected
        };
    }

    private sealed class BearerSecuritySchemeTransformer(
        IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
    {
        public async Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(document);

            IEnumerable<AuthenticationScheme> authenticationSchemes =
                await authenticationSchemeProvider.GetAllSchemesAsync();

            if (!authenticationSchemes.Any(scheme => scheme.Name == "Bearer"))
            {
                return;
            }

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    In = ParameterLocation.Header,
                    BearerFormat = "JWT",
                    Description = "Access token JWT Bearer emitido pelo Keycloak para a API PetShop."
                }
            };
        }
    }

    private sealed class StandardHttpOperationTransformer : IOpenApiOperationTransformer
    {
        public async Task TransformAsync(
            OpenApiOperation operation,
            OpenApiOperationTransformerContext context,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(operation);
            ArgumentNullException.ThrowIfNull(context);

            AddCorrelationHeader(operation);
            await AddProblemResponsesAsync(operation, context, cancellationToken);

            if (IsAnonymous(context))
            {
                return;
            }

            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
            });
        }

        private static void AddCorrelationHeader(OpenApiOperation operation)
        {
            operation.Parameters ??= [];

            if (operation.Parameters.Any(parameter =>
                    string.Equals(
                        parameter.Name,
                        PropagationHeaderNames.HttpCorrelationId,
                        StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = PropagationHeaderNames.HttpCorrelationId,
                In = ParameterLocation.Header,
                Required = false,
                Description = "Correlation ID operacional em formato GUID. Se ausente ou invalido, a API gera um novo valor e o devolve na resposta.",
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Format = "uuid"
                }
            });
        }

        private static async Task AddProblemResponsesAsync(
            OpenApiOperation operation,
            OpenApiOperationTransformerContext context,
            CancellationToken cancellationToken)
        {
            OpenApiSchema errorSchema =
                await context.GetOrCreateSchemaAsync(typeof(HttpValidationProblemDetails), null, cancellationToken);
            OpenApiDocument? document = context.Document;
            if (document is null)
            {
                return;
            }

            document.AddComponent("ProblemDetails", errorSchema);

            operation.Responses ??= new OpenApiResponses();
            AddProblemResponse(operation, "400", "Entrada invalida ou contrato HTTP malformado.", document);
            AddProblemResponse(operation, "401", "Token Bearer ausente ou invalido.", document);
            AddProblemResponse(operation, "403", "Principal autenticado sem autorizacao ou sem tenant valido.", document);
            AddProblemResponse(operation, "404", "Recurso inexistente ou nao visivel no escopo atual.", document);
            AddProblemResponse(operation, "409", "Conflito com o estado atual do recurso.", document);
            AddProblemResponse(operation, "500", "Falha inesperada sem detalhes internos.", document);
        }

        private static void AddProblemResponse(
            OpenApiOperation operation,
            string statusCode,
            string description,
            OpenApiDocument document)
        {
            OpenApiResponses responses = operation.Responses ??= new OpenApiResponses();

            responses[statusCode] = new OpenApiResponse
            {
                Description = description,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/problem+json"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchemaReference("ProblemDetails", document)
                    }
                }
            };
        }

        private static bool IsAnonymous(OpenApiOperationTransformerContext context)
        {
            return context.Description.ActionDescriptor.EndpointMetadata
                .OfType<IAllowAnonymous>()
                .Any();
        }
    }
}
