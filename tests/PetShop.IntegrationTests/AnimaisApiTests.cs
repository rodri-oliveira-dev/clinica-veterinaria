using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

using PetShop.Tutores.Domain;

namespace PetShop.IntegrationTests;

public sealed class AnimaisApiTests : IClassFixture<PostgreSqlFixture>, IDisposable
{
    private const string Issuer = "https://keycloak.test/realms/petshop-local";
    private const string Audience = "petshop-api";
    private const string RoleClientId = "petshop-api";
    private const string RequiredRole = "petshop.access";
    private const string TenantA = "11111111-1111-4111-8111-111111111111";
    private const string TenantB = "22222222-2222-4222-8222-222222222222";
    private const string KeyId = "animais-api-test-key";

    private readonly PostgreSqlFixture _postgresql;
    private readonly RSA _rsa = RSA.Create(2048);
    private readonly RsaSecurityKey _signingKey;
    private readonly PetShopApiFactory _factory;

    public AnimaisApiTests(PostgreSqlFixture postgresql)
    {
        _postgresql = postgresql;
        _signingKey = new RsaSecurityKey(_rsa)
        {
            KeyId = KeyId
        };
        _factory = new PetShopApiFactory(
            _postgresql.ConnectionString,
            new Dictionary<string, string?>
            {
                ["Authentication:Authority"] = Issuer,
                ["Authentication:Audience"] = Audience,
                ["Authentication:RoleClientId"] = RoleClientId,
                ["Authentication:RequiredRole"] = RequiredRole,
                ["Authentication:RequireHttpsMetadata"] = "true"
            },
            services => services.PostConfigure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.Configuration = new OpenIdConnectConfiguration
                    {
                        Issuer = Issuer
                    };
                    options.Configuration.SigningKeys.Add(_signingKey);
                    options.ConfigurationManager = null;
                    options.RefreshOnIssuerKeyNotFound = false;
                    options.TokenValidationParameters.ValidIssuer = Issuer;
                    options.TokenValidationParameters.ValidIssuers = [Issuer];
                    options.TokenValidationParameters.IssuerSigningKey = _signingKey;
                }));
    }

    [Fact]
    public async Task CadastrarAnimal_ComDadosValidos_RetornaCreatedComLocationEContratoMinimo()
    {
        await _postgresql.ResetDatabaseAsync();
        using HttpClient client = CriarClienteAutorizado(TenantA);
        Guid tutorId = await CadastrarTutorELerIdAsync(client, "Maria Oliveira", "529.982.247-25");

        using HttpResponseMessage response = await CadastrarAnimalAsync(client, tutorId, "  Luna  ", " Canina ");

        await AssertStatusCodeAsync(HttpStatusCode.Created, response);
        Assert.NotNull(response.Headers.Location);
        JsonElement animal = await ReadJsonAsync(response);
        Guid animalId = animal.GetProperty("animalId").GetGuid();
        Assert.Equal($"/animais/{animalId:D}", response.Headers.Location!.OriginalString);
        Assert.Equal(tutorId, animal.GetProperty("tutorResponsavelId").GetGuid());
        Assert.Equal("Luna", animal.GetProperty("nome").GetString());
        Assert.Equal("Canina", animal.GetProperty("especie").GetString());
        Assert.Equal("SRD", animal.GetProperty("raca").GetString());
        Assert.Equal("femea", animal.GetProperty("sexo").GetString());
        Assert.Equal("ativo", animal.GetProperty("situacao").GetString());
        Assert.False(animal.TryGetProperty("tenantId", out _));
        Assert.False(animal.TryGetProperty("tutor", out _));

        using HttpResponseMessage consulta = await client.GetAsync(
            response.Headers.Location,
            TestContext.Current.CancellationToken);

        await AssertStatusCodeAsync(HttpStatusCode.OK, consulta);
        JsonElement animalConsultado = await ReadJsonAsync(consulta);
        Assert.Equal(animalId, animalConsultado.GetProperty("animalId").GetGuid());
    }

    [Fact]
    public async Task CadastrarAnimal_ComEntradaInvalida_RetornaValidationProblemDetails()
    {
        await _postgresql.ResetDatabaseAsync();
        using HttpClient client = CriarClienteAutorizado(TenantA);

        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/animais",
            new
            {
                tutorResponsavelId = Guid.Empty,
                nome = " ",
                especie = " ",
                sexo = "desconhecido"
            },
            TestContext.Current.CancellationToken);

        await AssertStatusCodeAsync(HttpStatusCode.BadRequest, response);
        JsonElement problem = await AssertProblemDetailsAsync(response, HttpStatusCode.BadRequest, "request.invalid");
        JsonElement errors = problem.GetProperty("errors");
        Assert.True(errors.TryGetProperty("nome", out _));
        Assert.True(errors.TryGetProperty("especie", out _));
        Assert.True(errors.TryGetProperty("sexo", out _));
    }

    [Fact]
    public async Task CadastrarAnimal_ComTutorInexistente_RetornaNotFound()
    {
        await _postgresql.ResetDatabaseAsync();
        using HttpClient client = CriarClienteAutorizado(TenantA);

        using HttpResponseMessage response = await CadastrarAnimalAsync(client, Guid.NewGuid(), "Luna", "Canina");

        await AssertStatusCodeAsync(HttpStatusCode.NotFound, response);
        await AssertProblemDetailsAsync(response, HttpStatusCode.NotFound, "resource.not_found");
    }

    [Fact]
    public async Task CadastrarAnimal_ComTutorDeOutroTenant_RetornaNotFound()
    {
        await _postgresql.ResetDatabaseAsync();
        using HttpClient tenantA = CriarClienteAutorizado(TenantA);
        using HttpClient tenantB = CriarClienteAutorizado(TenantB);
        Guid tutorOutroTenant = await CadastrarTutorELerIdAsync(tenantB, "Bruna Lima", "123.456.789-09");

        using HttpResponseMessage response = await CadastrarAnimalAsync(tenantA, tutorOutroTenant, "Luna", "Canina");

        await AssertStatusCodeAsync(HttpStatusCode.NotFound, response);
        await AssertProblemDetailsAsync(response, HttpStatusCode.NotFound, "resource.not_found");
    }

    [Fact]
    public async Task ConsultarAnimal_DeOutroTenant_RetornaNotFound()
    {
        await _postgresql.ResetDatabaseAsync();
        using HttpClient tenantA = CriarClienteAutorizado(TenantA);
        using HttpClient tenantB = CriarClienteAutorizado(TenantB);
        Guid tutorId = await CadastrarTutorELerIdAsync(tenantA, "Maria Oliveira", "529.982.247-25");
        Guid animalId = await CadastrarAnimalELerIdAsync(tenantA, tutorId, "Luna", "Canina");

        using HttpResponseMessage response = await tenantB.GetAsync(
            $"/animais/{animalId:D}",
            TestContext.Current.CancellationToken);

        await AssertStatusCodeAsync(HttpStatusCode.NotFound, response);
        await AssertProblemDetailsAsync(response, HttpStatusCode.NotFound, "resource.not_found");
    }

    [Fact]
    public async Task AtualizarAnimal_AlteraCadastroSemAceitarTutorOuTenantNoBody()
    {
        await _postgresql.ResetDatabaseAsync();
        using HttpClient client = CriarClienteAutorizado(TenantA);
        Guid tutorId = await CadastrarTutorELerIdAsync(client, "Maria Oliveira", "529.982.247-25");
        Guid animalId = await CadastrarAnimalELerIdAsync(client, tutorId, "Luna", "Canina");

        using var payloadComCamposProibidos = new StringContent(
            $$"""
            {"animalId":"{{Guid.NewGuid():D}}","tenantId":"{{TenantB}}","tutorResponsavelId":"{{Guid.NewGuid():D}}","nome":"Sol","especie":"Felina"}
            """,
            Encoding.UTF8,
            "application/json");
        using HttpResponseMessage rejeitado = await client.PutAsync(
            $"/animais/{animalId:D}",
            payloadComCamposProibidos,
            TestContext.Current.CancellationToken);

        await AssertStatusCodeAsync(HttpStatusCode.BadRequest, rejeitado);
        await AssertProblemDetailsAsync(rejeitado, HttpStatusCode.BadRequest, "request.invalid");

        using HttpResponseMessage atualizado = await client.PutAsJsonAsync(
            $"/animais/{animalId:D}",
            new
            {
                nome = "Sol",
                especie = "Felina",
                raca = "Siames",
                sexo = "macho",
                dataDeNascimento = "2021-08-03",
                corOuPelagem = "Cinza",
                observacaoCadastral = "Aceita colo"
            },
            TestContext.Current.CancellationToken);

        await AssertStatusCodeAsync(HttpStatusCode.OK, atualizado);
        JsonElement animal = await ReadJsonAsync(atualizado);
        Assert.Equal(animalId, animal.GetProperty("animalId").GetGuid());
        Assert.Equal(tutorId, animal.GetProperty("tutorResponsavelId").GetGuid());
        Assert.Equal("Sol", animal.GetProperty("nome").GetString());
        Assert.Equal("Felina", animal.GetProperty("especie").GetString());
        Assert.Equal("Siames", animal.GetProperty("raca").GetString());
        Assert.Equal("macho", animal.GetProperty("sexo").GetString());
        Assert.Equal("2021-08-03", animal.GetProperty("dataDeNascimento").GetString());
    }

    [Fact]
    public async Task TransferirResponsabilidade_ComDadosValidos_AtualizaTutorEVersaoERegistraHistorico()
    {
        await _postgresql.ResetDatabaseAsync();
        using HttpClient client = CriarClienteAutorizado(TenantA);
        Guid tutorAnteriorId = await CadastrarTutorELerIdAsync(client, "Maria Oliveira", "529.982.247-25");
        Guid tutorNovoId = await CadastrarTutorELerIdAsync(client, "Bruna Lima", "123.456.789-09");
        Guid animalId = await CadastrarAnimalELerIdAsync(client, tutorAnteriorId, "Luna", "Canina");
        int versao = await ConsultarVersaoAnimalAsync(client, animalId);

        using HttpResponseMessage response = await TransferirResponsabilidadeAsync(
            client,
            animalId,
            tutorNovoId,
            versao,
            " Mudanca solicitada pelo tutor ");

        await AssertStatusCodeAsync(HttpStatusCode.OK, response);
        JsonElement animal = await ReadJsonAsync(response);
        Assert.Equal(animalId, animal.GetProperty("animalId").GetGuid());
        Assert.Equal(tutorNovoId, animal.GetProperty("tutorResponsavelId").GetGuid());
        Assert.Equal(versao + 1, animal.GetProperty("versao").GetInt32());
        Assert.False(animal.TryGetProperty("tenantId", out _));

        await using var dbContext = _postgresql.CreateDbContext(Guid.Parse(TenantA));
        TransferenciaDeResponsabilidadeDoAnimal historico = await dbContext
            .Set<TransferenciaDeResponsabilidadeDoAnimal>()
            .SingleAsync(TestContext.Current.CancellationToken);

        Assert.Equal(Guid.Parse(TenantA), historico.TenantId.Valor);
        Assert.Equal(animalId, historico.AnimalId.Valor);
        Assert.Equal(tutorAnteriorId, historico.TutorAnteriorId.Valor);
        Assert.Equal(tutorNovoId, historico.TutorNovoId.Valor);
        Assert.Equal("local-petshop-user", historico.Subject);
        Assert.Equal("Mudanca solicitada pelo tutor", historico.Motivo);
    }

    [Fact]
    public async Task TransferirResponsabilidade_ComTutorInativo_RetornaConflict()
    {
        await _postgresql.ResetDatabaseAsync();
        using HttpClient client = CriarClienteAutorizado(TenantA);
        Guid tutorAnteriorId = await CadastrarTutorELerIdAsync(client, "Maria Oliveira", "529.982.247-25");
        Guid tutorInativoId = await CadastrarTutorELerIdAsync(client, "Bruna Lima", "123.456.789-09");
        Guid animalId = await CadastrarAnimalELerIdAsync(client, tutorAnteriorId, "Luna", "Canina");
        int versao = await ConsultarVersaoAnimalAsync(client, animalId);

        using HttpResponseMessage inativacao = await client.PostAsync(
            $"/tutores/{tutorInativoId:D}/inativacao",
            content: null,
            TestContext.Current.CancellationToken);
        await AssertStatusCodeAsync(HttpStatusCode.OK, inativacao);

        using HttpResponseMessage response = await TransferirResponsabilidadeAsync(
            client,
            animalId,
            tutorInativoId,
            versao);

        await AssertStatusCodeAsync(HttpStatusCode.Conflict, response);
        await AssertProblemDetailsAsync(response, HttpStatusCode.Conflict, "resource.conflict");
    }

    [Fact]
    public async Task TransferirResponsabilidade_ComMesmoTutor_RetornaConflict()
    {
        await _postgresql.ResetDatabaseAsync();
        using HttpClient client = CriarClienteAutorizado(TenantA);
        Guid tutorId = await CadastrarTutorELerIdAsync(client, "Maria Oliveira", "529.982.247-25");
        Guid animalId = await CadastrarAnimalELerIdAsync(client, tutorId, "Luna", "Canina");
        int versao = await ConsultarVersaoAnimalAsync(client, animalId);

        using HttpResponseMessage response = await TransferirResponsabilidadeAsync(
            client,
            animalId,
            tutorId,
            versao);

        await AssertStatusCodeAsync(HttpStatusCode.Conflict, response);
        await AssertProblemDetailsAsync(response, HttpStatusCode.Conflict, "resource.conflict");
    }

    [Fact]
    public async Task TransferirResponsabilidade_ComDadosDeOutroTenant_RetornaNotFound()
    {
        await _postgresql.ResetDatabaseAsync();
        using HttpClient tenantA = CriarClienteAutorizado(TenantA);
        using HttpClient tenantB = CriarClienteAutorizado(TenantB);
        Guid tutorA = await CadastrarTutorELerIdAsync(tenantA, "Maria Oliveira", "529.982.247-25");
        Guid tutorB = await CadastrarTutorELerIdAsync(tenantB, "Ana Souza", "987.654.321-00");
        Guid animalA = await CadastrarAnimalELerIdAsync(tenantA, tutorA, "Luna", "Canina");
        int versao = await ConsultarVersaoAnimalAsync(tenantA, animalA);

        using HttpResponseMessage tutorCrossTenant = await TransferirResponsabilidadeAsync(
            tenantA,
            animalA,
            tutorB,
            versao);

        await AssertStatusCodeAsync(HttpStatusCode.NotFound, tutorCrossTenant);
        await AssertProblemDetailsAsync(tutorCrossTenant, HttpStatusCode.NotFound, "resource.not_found");

        using HttpResponseMessage animalCrossTenant = await TransferirResponsabilidadeAsync(
            tenantB,
            animalA,
            tutorB,
            versao);

        await AssertStatusCodeAsync(HttpStatusCode.NotFound, animalCrossTenant);
        await AssertProblemDetailsAsync(animalCrossTenant, HttpStatusCode.NotFound, "resource.not_found");
    }

    [Fact]
    public async Task TransferirResponsabilidade_ComVersaoDesatualizada_RetornaConflict()
    {
        await _postgresql.ResetDatabaseAsync();
        using HttpClient client = CriarClienteAutorizado(TenantA);
        Guid tutorAnteriorId = await CadastrarTutorELerIdAsync(client, "Maria Oliveira", "529.982.247-25");
        Guid tutorNovoId = await CadastrarTutorELerIdAsync(client, "Bruna Lima", "123.456.789-09");
        Guid outroTutorId = await CadastrarTutorELerIdAsync(client, "Ana Souza", "987.654.321-00");
        Guid animalId = await CadastrarAnimalELerIdAsync(client, tutorAnteriorId, "Luna", "Canina");
        int versao = await ConsultarVersaoAnimalAsync(client, animalId);

        using HttpResponseMessage primeiraTransferencia = await TransferirResponsabilidadeAsync(
            client,
            animalId,
            tutorNovoId,
            versao);
        await AssertStatusCodeAsync(HttpStatusCode.OK, primeiraTransferencia);

        using HttpResponseMessage response = await TransferirResponsabilidadeAsync(
            client,
            animalId,
            outroTutorId,
            versao);

        await AssertStatusCodeAsync(HttpStatusCode.Conflict, response);
        await AssertProblemDetailsAsync(response, HttpStatusCode.Conflict, "resource.conflict");
    }

    [Fact]
    public async Task PesquisarAnimais_AplicaFiltrosPaginacaoOrdenacaoEIsolamento()
    {
        await _postgresql.ResetDatabaseAsync();
        using HttpClient tenantA = CriarClienteAutorizado(TenantA);
        using HttpClient tenantB = CriarClienteAutorizado(TenantB);
        Guid tutorA = await CadastrarTutorELerIdAsync(tenantA, "Maria Oliveira", "529.982.247-25");
        Guid outroTutorA = await CadastrarTutorELerIdAsync(tenantA, "Bruna Lima", "123.456.789-09");
        Guid tutorB = await CadastrarTutorELerIdAsync(tenantB, "Ana Souza", "987.654.321-00");
        _ = await CadastrarAnimalELerIdAsync(tenantA, tutorA, "Bento", "Canina");
        _ = await CadastrarAnimalELerIdAsync(tenantA, tutorA, "Apolo", "Canina");
        Guid animalInativo = await CadastrarAnimalELerIdAsync(tenantA, outroTutorA, "Amora", "Felina");
        _ = await CadastrarAnimalELerIdAsync(tenantB, tutorB, "Apolo", "Canina");

        using HttpResponseMessage inativacao = await tenantA.PostAsync(
            $"/animais/{animalInativo:D}/inativacao",
            content: null,
            TestContext.Current.CancellationToken);
        await AssertStatusCodeAsync(HttpStatusCode.OK, inativacao);

        using HttpResponseMessage pagina = await tenantA.GetAsync(
            $"/animais?nome=A&pagina=1&tamanhoPagina=1&ordenarPor=nome&direcao=asc&tutorResponsavelId={tutorA:D}",
            TestContext.Current.CancellationToken);

        await AssertStatusCodeAsync(HttpStatusCode.OK, pagina);
        JsonElement resultado = await ReadJsonAsync(pagina);
        Assert.Equal(1, resultado.GetProperty("pagina").GetInt32());
        Assert.Equal(1, resultado.GetProperty("tamanhoPagina").GetInt32());
        Assert.Equal(1, resultado.GetProperty("total").GetInt32());
        JsonElement primeiroItem = resultado.GetProperty("itens")[0];
        Assert.Equal("Apolo", primeiroItem.GetProperty("nome").GetString());
        Assert.Equal(tutorA, primeiroItem.GetProperty("tutorResponsavelId").GetGuid());
        Assert.False(primeiroItem.TryGetProperty("raca", out _));

        using HttpResponseMessage porEspecieSituacao = await tenantA.GetAsync(
            "/animais?especie=Felina&situacao=inativo",
            TestContext.Current.CancellationToken);

        await AssertStatusCodeAsync(HttpStatusCode.OK, porEspecieSituacao);
        JsonElement filtro = await ReadJsonAsync(porEspecieSituacao);
        Assert.Equal(1, filtro.GetProperty("total").GetInt32());
        Assert.Equal("Amora", filtro.GetProperty("itens")[0].GetProperty("nome").GetString());
    }

    [Fact]
    public async Task InativarAnimal_MarcaInativoSemHardDelete()
    {
        await _postgresql.ResetDatabaseAsync();
        using HttpClient client = CriarClienteAutorizado(TenantA);
        Guid tutorId = await CadastrarTutorELerIdAsync(client, "Maria Oliveira", "529.982.247-25");
        Guid animalId = await CadastrarAnimalELerIdAsync(client, tutorId, "Luna", "Canina");

        using HttpResponseMessage response = await client.PostAsync(
            $"/animais/{animalId:D}/inativacao",
            content: null,
            TestContext.Current.CancellationToken);

        await AssertStatusCodeAsync(HttpStatusCode.OK, response);
        JsonElement animalInativo = await ReadJsonAsync(response);
        Assert.Equal("inativo", animalInativo.GetProperty("situacao").GetString());
        Assert.True(animalInativo.TryGetProperty("inativadoEm", out JsonElement inativadoEm));
        Assert.Equal(JsonValueKind.String, inativadoEm.ValueKind);

        using HttpResponseMessage consulta = await client.GetAsync(
            $"/animais/{animalId:D}",
            TestContext.Current.CancellationToken);

        await AssertStatusCodeAsync(HttpStatusCode.OK, consulta);
        JsonElement animalConsultado = await ReadJsonAsync(consulta);
        Assert.Equal("inativo", animalConsultado.GetProperty("situacao").GetString());
    }

    [Fact]
    public async Task Animais_ExigeAutenticacaoRoleETenantValido()
    {
        await _postgresql.ResetDatabaseAsync();
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage semToken = await client.GetAsync(
            "/animais",
            TestContext.Current.CancellationToken);
        await AssertStatusCodeAsync(HttpStatusCode.Unauthorized, semToken);
        await AssertProblemDetailsAsync(semToken, HttpStatusCode.Unauthorized, "auth.unauthenticated");

        using HttpResponseMessage transferenciaSemToken = await TransferirResponsabilidadeAsync(
            client,
            Guid.NewGuid(),
            Guid.NewGuid(),
            versao: 1);
        await AssertStatusCodeAsync(HttpStatusCode.Unauthorized, transferenciaSemToken);
        await AssertProblemDetailsAsync(
            transferenciaSemToken,
            HttpStatusCode.Unauthorized,
            "auth.unauthenticated");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CreateToken(roles: []));
        using HttpResponseMessage semRole = await client.GetAsync(
            "/animais",
            TestContext.Current.CancellationToken);
        await AssertStatusCodeAsync(HttpStatusCode.Forbidden, semRole);
        await AssertProblemDetailsAsync(semRole, HttpStatusCode.Forbidden, "auth.forbidden");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CreateToken(includeTenantClaim: false));
        using HttpResponseMessage semTenant = await client.GetAsync(
            "/animais",
            TestContext.Current.CancellationToken);
        await AssertStatusCodeAsync(HttpStatusCode.Forbidden, semTenant);
        await AssertProblemDetailsAsync(semTenant, HttpStatusCode.Forbidden, "identity.tenant_required");
    }

    public void Dispose()
    {
        _factory.Dispose();
        _rsa.Dispose();
    }

    private HttpClient CriarClienteAutorizado(string tenantId)
    {
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CreateToken(tenantId: tenantId));

        return client;
    }

    private static async Task<HttpResponseMessage> CadastrarTutorAsync(
        HttpClient client,
        string nome,
        string cpf) =>
        await client.PostAsJsonAsync(
            "/tutores",
            new
            {
                nome,
                cpf,
                email = $"{Guid.NewGuid():N}@example.com"
            },
            TestContext.Current.CancellationToken);

    private static async Task<Guid> CadastrarTutorELerIdAsync(
        HttpClient client,
        string nome,
        string cpf)
    {
        using HttpResponseMessage response = await CadastrarTutorAsync(client, nome, cpf);
        await AssertStatusCodeAsync(HttpStatusCode.Created, response);
        JsonElement tutor = await ReadJsonAsync(response);

        return tutor.GetProperty("tutorId").GetGuid();
    }

    private static async Task<HttpResponseMessage> CadastrarAnimalAsync(
        HttpClient client,
        Guid tutorResponsavelId,
        string nome,
        string especie) =>
        await client.PostAsJsonAsync(
            "/animais",
            new
            {
                tutorResponsavelId,
                nome,
                especie,
                raca = "SRD",
                sexo = "femea",
                dataDeNascimento = "2020-05-02",
                corOuPelagem = "Caramelo",
                observacaoCadastral = "Resgata com medo de fogos"
            },
            TestContext.Current.CancellationToken);

    private static async Task<Guid> CadastrarAnimalELerIdAsync(
        HttpClient client,
        Guid tutorResponsavelId,
        string nome,
        string especie)
    {
        using HttpResponseMessage response = await CadastrarAnimalAsync(client, tutorResponsavelId, nome, especie);
        await AssertStatusCodeAsync(HttpStatusCode.Created, response);
        JsonElement animal = await ReadJsonAsync(response);

        return animal.GetProperty("animalId").GetGuid();
    }

    private static async Task<HttpResponseMessage> TransferirResponsabilidadeAsync(
        HttpClient client,
        Guid animalId,
        Guid novoTutorId,
        int versao,
        string? motivo = null) =>
        await client.PostAsJsonAsync(
            $"/animais/{animalId:D}/transferencias-de-responsabilidade",
            new
            {
                novoTutorId,
                versao,
                motivo
            },
            TestContext.Current.CancellationToken);

    private static async Task<int> ConsultarVersaoAnimalAsync(HttpClient client, Guid animalId)
    {
        using HttpResponseMessage response = await client.GetAsync(
            $"/animais/{animalId:D}",
            TestContext.Current.CancellationToken);
        await AssertStatusCodeAsync(HttpStatusCode.OK, response);
        JsonElement animal = await ReadJsonAsync(response);

        return animal.GetProperty("versao").GetInt32();
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        await using Stream stream = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: TestContext.Current.CancellationToken);

        return document.RootElement.Clone();
    }

    private static async Task AssertStatusCodeAsync(
        HttpStatusCode expected,
        HttpResponseMessage response)
    {
        if (response.StatusCode == expected)
        {
            return;
        }

        string body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Fail($"Expected HTTP {(int)expected} {expected}, got {(int)response.StatusCode} {response.StatusCode}. Body: {body}");
    }

    private static async Task<JsonElement> AssertProblemDetailsAsync(
        HttpResponseMessage response,
        HttpStatusCode statusCode,
        string code)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        JsonElement problem = await ReadJsonAsync(response);
        Assert.Equal((int)statusCode, problem.GetProperty("status").GetInt32());
        Assert.Equal(code, problem.GetProperty("code").GetString());
        Assert.True(Guid.TryParse(problem.GetProperty("correlationId").GetString(), out _));
        Assert.DoesNotContain(TenantB, problem.ToString(), StringComparison.Ordinal);

        return problem;
    }

    private string CreateToken(
        IReadOnlyCollection<string>? roles = null,
        string tenantId = TenantA,
        bool includeTenantClaim = true)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var header = new Dictionary<string, object?>
        {
            ["alg"] = "RS256",
            ["kid"] = KeyId,
            ["typ"] = "JWT"
        };
        var payload = new Dictionary<string, object?>
        {
            ["iss"] = Issuer,
            ["aud"] = Audience,
            ["sub"] = "local-petshop-user",
            ["preferred_username"] = "local.petshop.user",
            ["iat"] = now.ToUnixTimeSeconds(),
            ["nbf"] = now.AddMinutes(-1).ToUnixTimeSeconds(),
            ["exp"] = now.AddMinutes(10).ToUnixTimeSeconds(),
            ["resource_access"] = new Dictionary<string, object?>
            {
                [RoleClientId] = new
                {
                    roles = roles ?? [RequiredRole]
                }
            }
        };

        if (includeTenantClaim)
        {
            payload["tenant_id"] = tenantId;
        }

        string encodedHeader = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header));
        string encodedPayload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload));
        string signingInput = string.Join('.', encodedHeader, encodedPayload);
        byte[] signature = _rsa.SignData(
            Encoding.UTF8.GetBytes(signingInput),
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        return string.Join('.', signingInput, Base64UrlEncode(signature));
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
