using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PetShop.Api.HttpApi;

public static class ApiProblemDetailsWriter
{
    public static Task WriteAsync(
        HttpContext httpContext,
        int statusCode,
        string title,
        string detail,
        string code)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        ProblemDetails problem = ApiProblemDetails.Create(
            httpContext,
            statusCode,
            title,
            detail,
            code);

        return WriteAsync(httpContext, problem);
    }

    public static async Task WriteAsync(HttpContext httpContext, ProblemDetails problem)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(problem);

        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;

        IProblemDetailsService? problemDetailsService =
            httpContext.RequestServices.GetService<IProblemDetailsService>();

        if (problemDetailsService is not null)
        {
            await problemDetailsService.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problem
            });

            return;
        }

        await Results.Problem(problem).ExecuteAsync(httpContext);
    }

    public static Task WriteStatusCodeProblemAsync(StatusCodeContext statusCodeContext)
    {
        ArgumentNullException.ThrowIfNull(statusCodeContext);

        HttpContext httpContext = statusCodeContext.HttpContext;
        if (httpContext.Response.HasStarted || httpContext.Response.StatusCode < StatusCodes.Status400BadRequest)
        {
            return Task.CompletedTask;
        }

        (string title, string detail, string code) = ResolveStatusProblem(httpContext.Response.StatusCode);

        return WriteAsync(httpContext, httpContext.Response.StatusCode, title, detail, code);
    }

    private static (string Title, string Detail, string Code) ResolveStatusProblem(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => (
                "Invalid request.",
                "The request is syntactically invalid or cannot be processed by the API contract.",
                ApiErrorCodes.InvalidRequest),
            StatusCodes.Status401Unauthorized => (
                "Authentication is required.",
                "A valid Bearer access credential is required to access this resource.",
                ApiErrorCodes.Unauthenticated),
            StatusCodes.Status403Forbidden => (
                "Access is forbidden.",
                "The authenticated principal is not authorized to access this resource.",
                ApiErrorCodes.Forbidden),
            StatusCodes.Status404NotFound => (
                "Resource was not found.",
                "The requested resource does not exist or is not visible in the current scope.",
                ApiErrorCodes.NotFound),
            StatusCodes.Status409Conflict => (
                "Resource conflict.",
                "The request conflicts with the current state of the resource.",
                ApiErrorCodes.Conflict),
            _ => (
                "Unexpected error.",
                "An unexpected error occurred while processing the request.",
                ApiErrorCodes.Unexpected)
        };
    }
}
