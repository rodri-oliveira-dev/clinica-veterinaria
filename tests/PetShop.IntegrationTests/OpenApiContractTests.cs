using System.Net;
using System.Text.Json;

namespace PetShop.IntegrationTests;

public sealed class OpenApiContractTests : IDisposable
{
    private readonly PetShopApiFactory _factory = new(
        "Host=localhost;Port=1;Database=petshop;Username=petshop;Password=petshop");

    [Fact]
    public async Task OpenApi_DescribesBearerCorrelationAndProblemDetails()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(
            "/openapi/v1.json",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using Stream stream = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: TestContext.Current.CancellationToken);
        JsonElement root = document.RootElement;

        JsonElement bearer = root
            .GetProperty("components")
            .GetProperty("securitySchemes")
            .GetProperty("Bearer");
        Assert.Equal("http", bearer.GetProperty("type").GetString());
        Assert.Equal("bearer", bearer.GetProperty("scheme").GetString());

        JsonElement diagnostics = root
            .GetProperty("paths")
            .GetProperty("/diagnostics")
            .GetProperty("get");
        Assert.Contains(
            diagnostics.GetProperty("parameters").EnumerateArray(),
            parameter =>
                parameter.GetProperty("name").GetString() == "X-Correlation-Id" &&
                parameter.GetProperty("in").GetString() == "header");
        Assert.True(diagnostics.TryGetProperty("security", out JsonElement security));
        Assert.NotEmpty(security.EnumerateArray());

        JsonElement responses = diagnostics.GetProperty("responses");
        Assert.True(responses.TryGetProperty("401", out JsonElement unauthorized));
        Assert.True(unauthorized
            .GetProperty("content")
            .TryGetProperty("application/problem+json", out _));
        Assert.True(responses.TryGetProperty("403", out _));
        Assert.True(responses.TryGetProperty("500", out _));
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}
