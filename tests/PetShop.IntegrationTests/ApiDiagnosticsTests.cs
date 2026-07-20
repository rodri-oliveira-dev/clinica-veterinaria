using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;

using PetShop.Observability.Propagation;

namespace PetShop.IntegrationTests;

public sealed class ApiDiagnosticsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiDiagnosticsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
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
    public async Task Diagnostics_ReturnsCurrentCorrelationId()
    {
        const string correlationId = "f2b935f0-b443-4c8d-8cdb-379e66c4c3f5";
        using HttpClient client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/diagnostics");
        request.Headers.TryAddWithoutValidation(PropagationHeaderNames.HttpCorrelationId, correlationId);

        using HttpResponseMessage response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        DiagnosticsResponse? body = await response.Content.ReadFromJsonAsync<DiagnosticsResponse>(
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal("PetShop.Api", body.Service);
        Assert.Equal(correlationId, body.CorrelationId);
    }

    private sealed record DiagnosticsResponse(
        string Service,
        string Environment,
        string? CorrelationId);
}
