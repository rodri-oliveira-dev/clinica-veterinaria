using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;

using PetShop.Observability.Propagation;

namespace PetShop.IntegrationTests;

public sealed class ApiDiagnosticsTests : IClassFixture<PostgreSqlFixture>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiDiagnosticsTests(PostgreSqlFixture postgresql)
    {
        ArgumentNullException.ThrowIfNull(postgresql);

        _factory = new PetShopApiFactory(postgresql.ConnectionString);
    }

    [Fact]
    public async Task Health_ReturnsOkAndCorrelationHeader()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues(PropagationHeaderNames.HttpCorrelationId, out IEnumerable<string>? values));
        Assert.True(Guid.TryParse(values.Single(), out _));
    }

    [Fact]
    public async Task Liveness_ReturnsOkWithoutDependingOnPostgreSql()
    {
        using var factory = new PetShopApiFactory(
            "Host=localhost;Port=1;Database=petshop;Username=petshop;Password=petshop");
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(
            "/health/live",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Readiness_ReturnsOkWhenPostgreSqlIsReady()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(
            "/health/ready",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Diagnostics_WithoutToken_ReturnsUnauthorizedAndCorrelationHeader()
    {
        const string correlationId = "f2b935f0-b443-4c8d-8cdb-379e66c4c3f5";
        using HttpClient client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/diagnostics");
        request.Headers.TryAddWithoutValidation(PropagationHeaderNames.HttpCorrelationId, correlationId);

        using HttpResponseMessage response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.True(response.Headers.TryGetValues(PropagationHeaderNames.HttpCorrelationId, out IEnumerable<string>? values));
        Assert.Equal(correlationId, values.Single());
        await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.Unauthorized,
            "auth.unauthenticated",
            correlationId);
    }

    [Fact]
    public async Task UnknownRoute_ReturnsProblemDetailsWithCorrelationId()
    {
        const string correlationId = "6f2485da-d93f-4683-a379-151b6c77ef61";
        using HttpClient client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/does-not-exist");
        request.Headers.TryAddWithoutValidation(PropagationHeaderNames.HttpCorrelationId, correlationId);

        using HttpResponseMessage response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertProblemDetailsAsync(response, HttpStatusCode.NotFound, "resource.not_found", correlationId);
    }

    private static async Task AssertProblemDetailsAsync(
        HttpResponseMessage response,
        HttpStatusCode statusCode,
        string code,
        string correlationId)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        await using Stream stream = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: TestContext.Current.CancellationToken);
        JsonElement root = document.RootElement;

        Assert.Equal((int)statusCode, root.GetProperty("status").GetInt32());
        Assert.Equal(code, root.GetProperty("code").GetString());
        Assert.Equal(correlationId, root.GetProperty("correlationId").GetString());

        string body = root.ToString();
        Assert.DoesNotContain("stack", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("connection string", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("token", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("claims", body, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}
