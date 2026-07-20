using System.Net;

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
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}
