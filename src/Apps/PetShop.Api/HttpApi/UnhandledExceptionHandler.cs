using Microsoft.AspNetCore.Diagnostics;

namespace PetShop.Api.HttpApi;

internal sealed class UnhandledExceptionHandler : IExceptionHandler
{
    private static readonly Action<ILogger, Exception?> LogUnhandledHttpException =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(1, nameof(UnhandledExceptionHandler)),
            "Unhandled exception while processing HTTP request.");

    private readonly ILogger<UnhandledExceptionHandler> _logger;

    public UnhandledExceptionHandler(ILogger<UnhandledExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);

        if (exception is BadHttpRequestException badRequestException)
        {
            await ApiProblemDetailsWriter.WriteAsync(
                httpContext,
                badRequestException.StatusCode,
                "Invalid request.",
                "The request is syntactically invalid or cannot be processed by the API contract.",
                ApiErrorCodes.InvalidRequest);

            return true;
        }

        LogUnhandledHttpException(_logger, exception);

        await ApiProblemDetailsWriter.WriteAsync(
            httpContext,
            StatusCodes.Status500InternalServerError,
            "Unexpected error.",
            "An unexpected error occurred while processing the request.",
            ApiErrorCodes.Unexpected);

        return true;
    }
}
