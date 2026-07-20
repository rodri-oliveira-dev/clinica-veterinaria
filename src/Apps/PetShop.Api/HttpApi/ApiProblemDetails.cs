using Microsoft.AspNetCore.Mvc;

namespace PetShop.Api.HttpApi;

public static class ApiProblemDetails
{
    public static ProblemDetails Create(
        HttpContext httpContext,
        int statusCode,
        string title,
        string detail,
        string code)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(detail);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var problem = new ProblemDetails
        {
            Type = $"urn:petshop:error:{code}",
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = httpContext.Request.Path.HasValue
                ? httpContext.Request.Path.Value
                : null
        };

        problem.Extensions["code"] = code;
        problem.Extensions["correlationId"] = ResolveCorrelationId(httpContext);

        return problem;
    }

    public static string ResolveCorrelationId(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        string? responseHeader = httpContext.Response.Headers[PetShop.Observability.Propagation.PropagationHeaderNames.HttpCorrelationId]
            .FirstOrDefault();
        if (Guid.TryParse(responseHeader, out Guid responseCorrelationId))
        {
            return responseCorrelationId.ToString();
        }

        string? requestHeader = httpContext.Request.Headers[PetShop.Observability.Propagation.PropagationHeaderNames.HttpCorrelationId]
            .FirstOrDefault();
        if (Guid.TryParse(requestHeader, out Guid requestCorrelationId))
        {
            return requestCorrelationId.ToString();
        }

        return httpContext.TraceIdentifier;
    }
}
