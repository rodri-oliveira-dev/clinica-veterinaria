using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace PetShop.IntegrationTests;

public sealed class TutoresApiTests : IClassFixture<PostgreSqlFixture>, IDisposable
{
    private const string Issuer = "https://keycloak.test/realms/petshop-local";
    private const string Audience = "petshop-api";
    private const string RoleClientId = "petshop-api";
    private const string RequiredRole = "petshop.access";
    private const string TenantA = "11111111-1111-4111-8111-111111111111";
    private const string TenantB = "22222222-2222-4222-8222-222222222222";
    private const string KeyId = "tutores-api-test-key";

    private readonly PostgreSqlFixture _postgresql;
    private readonly RSA _rsa = RSA.Create(2048);
    private readonly RsaSecurityKey _signingKey;
    private readonly PetShopApiFactory _factory;

    public TutoresApiTests(PostgreSqlFixture postgresql)
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
    public async Task CadastrarTutor_ComDadosValidos_RetornaCreatedComLocationEContratoMascarado()
    {
        await _postgresql.ResetDatabaseAsync();
        using HttpClient client = CriarClienteAutorizado(TenantA);

        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/tutores",
            new
            {
                nome = "  Maria Oliveira  ",
                cpf = "529.982.247-25",
                email = "Maria@Example.com",
                telefone = "+55 (11) 98765-4321"
            },
            TestContext.Current.CancellationToken);

        await AssertStatusCodeAsync(HttpStatusCode.Created, response);
        Assert.NotNull(response.Headers.Location);
        JsonElement tutor = await ReadJsonAsync(response);
        Guid tutorId = tutor.GetProperty("tutorId").GetGuid();
        Assert.Equal($"/tutores/{tutorId:D}", response.Headers.Location!.OriginalString);
        Assert.Equal("Maria Oliveira", tutor.GetProperty("nome").GetString());
        Assert.Equal("***.***.***-25", tutor.GetProperty("cpfMascarado").GetString());
        Assert.Equal("maria@example.com", tutor.GetProperty("email").GetString());
        Assert.Equal("11987654321", tutor.GetProperty("telefone").GetString());
        Assert.Equal("ativo", tutor.GetProperty("situacao").GetString());
        Assert.False(tutor.TryGetProperty("tenantId", out _));
        Assert.False(tutor.TryGetProperty("cpf", out _));

        using HttpResponseMessage consulta = await client.GetAsync(
            response.Headers.Location,
            TestContext.Current.CancellationToken);

        await AssertStatusCodeAsync(HttpStatusCode.OK, consulta);
        JsonElement tutorConsultado = await ReadJsonAsync(consulta);
        Assert.Equal(tutorId, tutorConsultado.GetProperty("tutorId").GetGuid());
    }

    [Fact]
    public async Task CadastrarTutor_ComEntradaInvalida_RetornaValidationProblemDetails()
    {
        await _postgresql.ResetDatabaseAsync();
        using HttpClient client = CriarClienteAutorizado(TenantA);

        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/tutores",
            new
            {
                nome = " ",
                cpf = "111.111.111-11"
            },
            TestContext.Current.CancellationToken);

        await AssertStatusCodeAsync(HttpStatusCode.BadRequest, response);
        JsonElement problem = await AssertProblemDetailsAsync(response, HttpStatusCode.BadRequest, "request.invalid");
        JsonElement errors = problem.GetProperty("errors");
        Assert.True(errors.TryGetProperty("nome", out _));
        Assert.True(errors.TryGetProperty("cpf", out _));
        Assert.True(errors.TryGetProperty("contato", out _));
    }

    [Fact]
    public async Task CadastrarTutor_ComCpfDuplicado_RespeitaEscopoDoTenant()
    {
        await _postgresql.ResetDatabaseAsync();
        using HttpClient tenantA = CriarClienteAutorizado(TenantA);
        using HttpClient tenantB = CriarClienteAutorizado(TenantB);

        using HttpResponseMessage primeiro = await CadastrarTutorAsync(
            tenantA,
            "Maria Oliveira",
            "529.982.247-25");
        await AssertStatusCodeAsync(HttpStatusCode.Created, primeiro);

        using HttpResponseMessage duplicadoNoMesmoTenant = await CadastrarTutorAsync(
            tenantA,
            "Maria Duplicada",
            "52998224725");
        await AssertStatusCodeAsync(HttpStatusCode.Conflict, duplicadoNoMesmoTenant);
        await AssertProblemDetailsAsync(
            duplicadoNoMesmoTenant,
            HttpStatusCode.Conflict,
            "resource.conflict");

        using HttpResponseMessage mesmoCpfEmOutroTenant = await CadastrarTutorAsync(
            tenantB,
            "Maria Outro Tenant",
            "52998224725");
        await AssertStatusCodeAsync(HttpStatusCode.Created, mesmoCpfEmOutroTenant);
    }

    [Fact]
    public async Task ConsultarTutor_DeOutroTenant_RetornaNotFound()
    {
        await _postgresql.ResetDatabaseAsync();
        using HttpClient tenantA = CriarClienteAutorizado(TenantA);
        using HttpClient tenantB = CriarClienteAutorizado(TenantB);
        Guid tutorId = await CadastrarTutorELerIdAsync(tenantA, "Maria Oliveira", "529.982.247-25");

        using HttpResponseMessage response = await tenantB.GetAsync(
            $"/tutores/{tutorId:D}",
            TestContext.Current.CancellationToken);

        await AssertStatusCodeAsync(HttpStatusCode.NotFound, response);
        await AssertProblemDetailsAsync(response, HttpStatusCode.NotFound, "resource.not_found");
    }

    [Fact]
    public async Task AtualizarTutor_AlteraCadastroSemAceitarIdOuTenantNoBody()
    {
        await _postgresql.ResetDatabaseAsync();
        using HttpClient client = CriarClienteAutorizado(TenantA);
        Guid tutorId = await CadastrarTutorELerIdAsync(client, "Maria Oliveira", "529.982.247-25");

        using var payloadComCamposProibidos = new StringContent(
            $$"""
            {"tutorId":"{{Guid.NewGuid():D}}","tenantId":"{{TenantB}}","nome":"Maria Santos","email":"maria.santos@example.com"}
            """,
            Encoding.UTF8,
            "application/json");
        using HttpResponseMessage rejeitado = await client.PutAsync(
            $"/tutores/{tutorId:D}",
            payloadComCamposProibidos,
            TestContext.Current.CancellationToken);

        await AssertStatusCodeAsync(HttpStatusCode.BadRequest, rejeitado);
        await AssertProblemDetailsAsync(rejeitado, HttpStatusCode.BadRequest, "request.invalid");

        using HttpResponseMessage atualizado = await client.PutAsJsonAsync(
            $"/tutores/{tutorId:D}",
            new
            {
                nome = "Maria Santos",
                cpf = "123.456.789-09",
                email = "Maria.Santos@Example.com",
                telefone = "(21) 3333-4444"
            },
            TestContext.Current.CancellationToken);

        await AssertStatusCodeAsync(HttpStatusCode.OK, atualizado);
        JsonElement tutor = await ReadJsonAsync(atualizado);
        Assert.Equal(tutorId, tutor.GetProperty("tutorId").GetGuid());
        Assert.Equal("Maria Santos", tutor.GetProperty("nome").GetString());
        Assert.Equal("***.***.***-09", tutor.GetProperty("cpfMascarado").GetString());
        Assert.Equal("maria.santos@example.com", tutor.GetProperty("email").GetString());
        Assert.Equal("2133334444", tutor.GetProperty("telefone").GetString());
    }

    [Fact]
    public async Task PesquisarTutores_AplicaFiltrosPaginacaoOrdenacaoEIsolamento()
    {
        await _postgresql.ResetDatabaseAsync();
        using HttpClient tenantA = CriarClienteAutorizado(TenantA);
        using HttpClient tenantB = CriarClienteAutorizado(TenantB);
        _ = await CadastrarTutorELerIdAsync(tenantA, "Bruna Lima", "123.456.789-09");
        _ = await CadastrarTutorELerIdAsync(tenantA, "Ana Souza", "987.654.321-00");
        Guid tutorInativo = await CadastrarTutorELerIdAsync(tenantA, "Carla Souza", "234.567.891-73");
        _ = await CadastrarTutorELerIdAsync(tenantB, "Ana Souza", "345.678.912-28");

        using HttpResponseMessage inativacao = await tenantA.PostAsync(
            $"/tutores/{tutorInativo:D}/inativacao",
            content: null,
            TestContext.Current.CancellationToken);
        await AssertStatusCodeAsync(HttpStatusCode.OK, inativacao);

        using HttpResponseMessage pagina = await tenantA.GetAsync(
            "/tutores?nome=Souza&pagina=1&tamanhoPagina=1&ordenarPor=nome&direcao=asc",
            TestContext.Current.CancellationToken);

        await AssertStatusCodeAsync(HttpStatusCode.OK, pagina);
        JsonElement resultado = await ReadJsonAsync(pagina);
        Assert.Equal(1, resultado.GetProperty("pagina").GetInt32());
        Assert.Equal(1, resultado.GetProperty("tamanhoPagina").GetInt32());
        Assert.Equal(2, resultado.GetProperty("total").GetInt32());
        JsonElement primeiroItem = resultado.GetProperty("itens")[0];
        Assert.Equal("Ana Souza", primeiroItem.GetProperty("nome").GetString());
        Assert.False(primeiroItem.TryGetProperty("email", out _));

        using HttpResponseMessage porCpf = await tenantA.GetAsync(
            "/tutores?cpf=234.567.891-73&situacao=inativo",
            TestContext.Current.CancellationToken);

        await AssertStatusCodeAsync(HttpStatusCode.OK, porCpf);
        JsonElement filtroCpf = await ReadJsonAsync(porCpf);
        Assert.Equal(1, filtroCpf.GetProperty("total").GetInt32());
        Assert.Equal("Carla Souza", filtroCpf.GetProperty("itens")[0].GetProperty("nome").GetString());
    }

    [Fact]
    public async Task InativarTutor_MarcaInativoSemHardDelete()
    {
        await _postgresql.ResetDatabaseAsync();
        using HttpClient client = CriarClienteAutorizado(TenantA);
        Guid tutorId = await CadastrarTutorELerIdAsync(client, "Maria Oliveira", "529.982.247-25");

        using HttpResponseMessage response = await client.PostAsync(
            $"/tutores/{tutorId:D}/inativacao",
            content: null,
            TestContext.Current.CancellationToken);

        await AssertStatusCodeAsync(HttpStatusCode.OK, response);
        JsonElement tutorInativo = await ReadJsonAsync(response);
        Assert.Equal("inativo", tutorInativo.GetProperty("situacao").GetString());
        Assert.True(tutorInativo.TryGetProperty("inativadoEm", out JsonElement inativadoEm));
        Assert.Equal(JsonValueKind.String, inativadoEm.ValueKind);

        using HttpResponseMessage consulta = await client.GetAsync(
            $"/tutores/{tutorId:D}",
            TestContext.Current.CancellationToken);

        await AssertStatusCodeAsync(HttpStatusCode.OK, consulta);
        JsonElement tutorConsultado = await ReadJsonAsync(consulta);
        Assert.Equal("inativo", tutorConsultado.GetProperty("situacao").GetString());
    }

    [Fact]
    public async Task InativarTutor_ComAnimalAtivoVinculado_RetornaConflictEPreservaTutor()
    {
        await _postgresql.ResetDatabaseAsync();
        using HttpClient client = CriarClienteAutorizado(TenantA);
        Guid tutorId = await CadastrarTutorELerIdAsync(client, "Maria Oliveira", "529.982.247-25");
        _ = await CadastrarAnimalELerIdAsync(client, tutorId, "Luna", "Canina");

        using HttpResponseMessage response = await client.PostAsync(
            $"/tutores/{tutorId:D}/inativacao",
            content: null,
            TestContext.Current.CancellationToken);

        await AssertStatusCodeAsync(HttpStatusCode.Conflict, response);
        await AssertProblemDetailsAsync(response, HttpStatusCode.Conflict, "resource.conflict");

        using HttpResponseMessage consulta = await client.GetAsync(
            $"/tutores/{tutorId:D}",
            TestContext.Current.CancellationToken);

        await AssertStatusCodeAsync(HttpStatusCode.OK, consulta);
        JsonElement tutor = await ReadJsonAsync(consulta);
        Assert.Equal("ativo", tutor.GetProperty("situacao").GetString());
    }

    [Fact]
    public async Task Tutores_ExigeAutenticacaoRoleETenantValido()
    {
        await _postgresql.ResetDatabaseAsync();
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage semToken = await client.GetAsync(
            "/tutores",
            TestContext.Current.CancellationToken);
        await AssertStatusCodeAsync(HttpStatusCode.Unauthorized, semToken);
        await AssertProblemDetailsAsync(semToken, HttpStatusCode.Unauthorized, "auth.unauthenticated");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CreateToken(roles: []));
        using HttpResponseMessage semRole = await client.GetAsync(
            "/tutores",
            TestContext.Current.CancellationToken);
        await AssertStatusCodeAsync(HttpStatusCode.Forbidden, semRole);
        await AssertProblemDetailsAsync(semRole, HttpStatusCode.Forbidden, "auth.forbidden");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CreateToken(includeTenantClaim: false));
        using HttpResponseMessage semTenant = await client.GetAsync(
            "/tutores",
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
                sexo = "naoInformado"
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
        Assert.DoesNotContain("52998224725", problem.ToString(), StringComparison.Ordinal);

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
