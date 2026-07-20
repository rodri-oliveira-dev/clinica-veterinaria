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

        JsonElement paths = root.GetProperty("paths");
        Assert.True(paths.GetProperty("/tutores").TryGetProperty("post", out JsonElement cadastrarTutor));
        Assert.True(paths.GetProperty("/tutores").TryGetProperty("get", out JsonElement pesquisarTutores));
        Assert.True(paths.GetProperty("/tutores/{tutorId}").TryGetProperty("get", out JsonElement consultarTutor));
        Assert.True(paths.GetProperty("/tutores/{tutorId}").TryGetProperty("put", out JsonElement atualizarTutor));
        Assert.True(paths.GetProperty("/tutores/{tutorId}/inativacao").TryGetProperty("post", out JsonElement inativarTutor));
        Assert.True(paths.GetProperty("/animais").TryGetProperty("post", out JsonElement cadastrarAnimal));
        Assert.True(paths.GetProperty("/animais").TryGetProperty("get", out JsonElement pesquisarAnimais));
        Assert.True(paths.GetProperty("/animais/{animalId}").TryGetProperty("get", out JsonElement consultarAnimal));
        Assert.True(paths.GetProperty("/animais/{animalId}").TryGetProperty("put", out JsonElement atualizarAnimal));
        Assert.True(paths.GetProperty("/animais/{animalId}/inativacao").TryGetProperty("post", out JsonElement inativarAnimal));

        AssertSecuredModuleOperation(cadastrarTutor);
        AssertSecuredModuleOperation(pesquisarTutores);
        AssertSecuredModuleOperation(consultarTutor);
        AssertSecuredModuleOperation(atualizarTutor);
        AssertSecuredModuleOperation(inativarTutor);
        AssertSecuredModuleOperation(cadastrarAnimal);
        AssertSecuredModuleOperation(pesquisarAnimais);
        AssertSecuredModuleOperation(consultarAnimal);
        AssertSecuredModuleOperation(atualizarAnimal);
        AssertSecuredModuleOperation(inativarAnimal);
        Assert.Contains(
            consultarTutor.GetProperty("parameters").EnumerateArray(),
            parameter =>
                parameter.GetProperty("name").GetString() == "tutorId" &&
                parameter.GetProperty("in").GetString() == "path");
        Assert.Contains(
            consultarAnimal.GetProperty("parameters").EnumerateArray(),
            parameter =>
                parameter.GetProperty("name").GetString() == "animalId" &&
                parameter.GetProperty("in").GetString() == "path");
    }

    private static void AssertSecuredModuleOperation(JsonElement operation)
    {
        Assert.True(operation.TryGetProperty("security", out JsonElement security));
        Assert.NotEmpty(security.EnumerateArray());

        JsonElement responses = operation.GetProperty("responses");
        Assert.True(responses.TryGetProperty("401", out _));
        Assert.True(responses.TryGetProperty("403", out _));
        Assert.True(responses.TryGetProperty("500", out _));
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}
