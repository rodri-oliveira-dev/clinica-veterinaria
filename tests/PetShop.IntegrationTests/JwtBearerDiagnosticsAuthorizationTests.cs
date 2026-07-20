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

public sealed class JwtBearerDiagnosticsAuthorizationTests : IDisposable
{
    private const string Issuer = "https://keycloak.test/realms/petshop-local";
    private const string Audience = "petshop-api";
    private const string RoleClientId = "petshop-api";
    private const string RequiredRole = "petshop.access";
    private const string TenantA = "11111111-1111-4111-8111-111111111111";
    private const string TenantB = "22222222-2222-4222-8222-222222222222";
    private const string KeyId = "integration-test-key";
    private const string DummyConnectionString =
        "Host=localhost;Port=5432;Database=petshop;Username=petshop;Password=petshop";

    private readonly RSA _rsa = RSA.Create(2048);
    private readonly RsaSecurityKey _signingKey;
    private readonly PetShopApiFactory _factory;

    public JwtBearerDiagnosticsAuthorizationTests()
    {
        _signingKey = new RsaSecurityKey(_rsa)
        {
            KeyId = KeyId
        };

        _factory = new PetShopApiFactory(
            DummyConnectionString,
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
                    options.TokenValidationParameters.ValidateIssuer = true;
                    options.TokenValidationParameters.ValidIssuer = Issuer;
                    options.TokenValidationParameters.ValidIssuers = [Issuer];
                    options.TokenValidationParameters.IssuerSigningKey = _signingKey;
                }));
    }

    [Fact]
    public async Task Diagnostics_WithoutToken_ReturnsUnauthorized()
    {
        using HttpClient client = _factory.CreateClient();
        const string correlationId = "0c822450-56e3-4ffd-9079-9d04e7e74dcb";

        using var request = new HttpRequestMessage(HttpMethod.Get, "/diagnostics");
        request.Headers.TryAddWithoutValidation("X-Correlation-Id", correlationId);
        using HttpResponseMessage response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        AssertStatusCode(HttpStatusCode.Unauthorized, response);
        await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.Unauthorized,
            "auth.unauthenticated",
            correlationId);
    }

    [Theory]
    [InlineData(InvalidTokenKind.Malformed)]
    [InlineData(InvalidTokenKind.Expired)]
    [InlineData(InvalidTokenKind.WrongIssuer)]
    [InlineData(InvalidTokenKind.WrongAudience)]
    [InlineData(InvalidTokenKind.WrongSignature)]
    public async Task Diagnostics_WithInvalidToken_ReturnsUnauthorized(InvalidTokenKind tokenKind)
    {
        using HttpClient client = _factory.CreateClient();
        string token = CreateInvalidToken(tokenKind);

        using HttpResponseMessage response = await SendDiagnosticsAsync(client, token);

        AssertStatusCode(HttpStatusCode.Unauthorized, response);
    }

    [Fact]
    public async Task Diagnostics_WithValidTokenWithoutRequiredRole_ReturnsForbidden()
    {
        using HttpClient client = _factory.CreateClient();
        string token = CreateToken(roles: []);

        using HttpResponseMessage response = await SendDiagnosticsAsync(client, token);

        AssertStatusCode(HttpStatusCode.Forbidden, response);
        await AssertProblemDetailsAsync(response, HttpStatusCode.Forbidden, "auth.forbidden");
    }

    [Fact]
    public async Task Diagnostics_WithValidAuthorizedTokenWithoutTenant_ReturnsForbidden()
    {
        using HttpClient client = _factory.CreateClient();
        string token = CreateToken(includeTenantClaim: false);

        using HttpResponseMessage response = await SendDiagnosticsAsync(client, token);

        AssertStatusCode(HttpStatusCode.Forbidden, response);
        await AssertProblemDetailsAsync(response, HttpStatusCode.Forbidden, "identity.tenant_required");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Diagnostics_WithValidAuthorizedTokenWithInvalidTenant_ReturnsForbidden(string tenantId)
    {
        using HttpClient client = _factory.CreateClient();
        string token = CreateToken(tenantId: tenantId);

        using HttpResponseMessage response = await SendDiagnosticsAsync(client, token);

        AssertStatusCode(HttpStatusCode.Forbidden, response);
        await AssertProblemDetailsAsync(response, HttpStatusCode.Forbidden, "identity.tenant_required");
    }

    [Fact]
    public async Task Diagnostics_WithValidAuthorizedToken_ReturnsCurrentTenantContextWithoutClaimsDump()
    {
        using HttpClient client = _factory.CreateClient();
        string token = CreateToken(roles: [RequiredRole], tenantId: TenantA);
        const string correlationId = "75db3340-c2e7-4dd5-886b-6c39530bd88c";

        using HttpResponseMessage response = await SendDiagnosticsAsync(client, token, correlationId);

        AssertStatusCode(HttpStatusCode.OK, response);
        DiagnosticsResponse? body = await response.Content.ReadFromJsonAsync<DiagnosticsResponse>(
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal("PetShop.Api", body.Service);
        Assert.Equal("Development", body.Environment);
        Assert.Equal(TenantA, body.TenantId);
        Assert.Equal(correlationId, body.CorrelationId);
    }

    [Fact]
    public async Task Diagnostics_WithTenantOverrideAttempts_ReturnsClaimTenant()
    {
        using HttpClient client = _factory.CreateClient();
        string token = CreateToken(tenantId: TenantB);

        using HttpResponseMessage response = await SendDiagnosticsAsync(
            client,
            token,
            tenantOverride: TenantA);

        AssertStatusCode(HttpStatusCode.OK, response);
        DiagnosticsResponse? body = await response.Content.ReadFromJsonAsync<DiagnosticsResponse>(
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(TenantB, body.TenantId);
    }

    [Fact]
    public async Task Diagnostics_WithTwoTenantsInParallel_DoesNotLeakTenantContext()
    {
        using HttpClient client = _factory.CreateClient();
        string tokenA = CreateToken(tenantId: TenantA);
        string tokenB = CreateToken(tenantId: TenantB);

        Task<DiagnosticsResponse>[] requests = Enumerable
            .Range(0, 20)
            .Select(index =>
            {
                string expectedTenant = index % 2 == 0 ? TenantA : TenantB;
                string token = index % 2 == 0 ? tokenA : tokenB;
                string overrideTenant = index % 2 == 0 ? TenantB : TenantA;
                string correlationId = Guid.NewGuid().ToString();

                return SendAndReadDiagnosticsAsync(
                    client,
                    token,
                    correlationId,
                    overrideTenant,
                    expectedTenant);
            })
            .ToArray();

        DiagnosticsResponse[] responses = await Task.WhenAll(requests);

        Assert.Equal(10, responses.Count(response => response.TenantId == TenantA));
        Assert.Equal(10, responses.Count(response => response.TenantId == TenantB));
    }

    public void Dispose()
    {
        _factory.Dispose();
        _rsa.Dispose();
    }

    private static async Task<HttpResponseMessage> SendDiagnosticsAsync(
        HttpClient client,
        string token,
        string? correlationId = null,
        string? tenantOverride = null)
    {
        string uri = tenantOverride is null
            ? "/diagnostics"
            : $"/diagnostics?tenant_id={Uri.EscapeDataString(tenantOverride)}";

        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (correlationId is not null)
        {
            request.Headers.TryAddWithoutValidation("X-Correlation-Id", correlationId);
        }

        if (tenantOverride is not null)
        {
            request.Headers.TryAddWithoutValidation("tenant_id", tenantOverride);
            request.Headers.TryAddWithoutValidation("X-Tenant-Id", tenantOverride);
        }

        return await client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private static async Task<DiagnosticsResponse> SendAndReadDiagnosticsAsync(
        HttpClient client,
        string token,
        string correlationId,
        string tenantOverride,
        string expectedTenant)
    {
        using HttpResponseMessage response = await SendDiagnosticsAsync(
            client,
            token,
            correlationId,
            tenantOverride);

        AssertStatusCode(HttpStatusCode.OK, response);
        DiagnosticsResponse? body = await response.Content.ReadFromJsonAsync<DiagnosticsResponse>(
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(expectedTenant, body.TenantId);
        Assert.Equal(correlationId, body.CorrelationId);

        return body;
    }

    private static void AssertStatusCode(
        HttpStatusCode expected,
        HttpResponseMessage response)
    {
        string challenge = string.Join(", ", response.Headers.WwwAuthenticate.Select(value => value.ToString()));

        Assert.True(
            response.StatusCode == expected,
            $"Expected HTTP {(int)expected} {expected}, got {(int)response.StatusCode} {response.StatusCode}. " +
            $"WWW-Authenticate: {challenge}");
    }

    private static async Task AssertProblemDetailsAsync(
        HttpResponseMessage response,
        HttpStatusCode statusCode,
        string code,
        string? expectedCorrelationId = null)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using JsonDocument document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken),
            cancellationToken: TestContext.Current.CancellationToken);
        JsonElement root = document.RootElement;

        Assert.Equal((int)statusCode, root.GetProperty("status").GetInt32());
        Assert.Equal(code, root.GetProperty("code").GetString());
        Assert.True(Guid.TryParse(root.GetProperty("correlationId").GetString(), out _));

        if (expectedCorrelationId is not null)
        {
            Assert.Equal(expectedCorrelationId, root.GetProperty("correlationId").GetString());
        }

        string body = root.ToString();
        Assert.DoesNotContain("stack", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("connection string", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("token", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("claims", body, StringComparison.OrdinalIgnoreCase);
    }

    private string CreateInvalidToken(InvalidTokenKind tokenKind)
    {
        return tokenKind switch
        {
            InvalidTokenKind.Malformed => "not-a-jwt",
            InvalidTokenKind.Expired => CreateToken(expiresAt: DateTimeOffset.UtcNow.AddMinutes(-5)),
            InvalidTokenKind.WrongIssuer => CreateToken(issuer: "https://keycloak.test/realms/other"),
            InvalidTokenKind.WrongAudience => CreateToken(audience: "other-api"),
            InvalidTokenKind.WrongSignature => CreateTokenWithWrongSignature(),
            _ => throw new ArgumentOutOfRangeException(nameof(tokenKind), tokenKind, null)
        };
    }

    private string CreateToken(
        string issuer = Issuer,
        string audience = Audience,
        DateTimeOffset? expiresAt = null,
        IReadOnlyCollection<string>? roles = null,
        string tenantId = TenantA,
        bool includeTenantClaim = true)
    {
        return CreateTokenCore(_rsa, issuer, audience, expiresAt, roles, tenantId, includeTenantClaim, KeyId);
    }

    private static string CreateTokenCore(
        RSA signingKey,
        string issuer,
        string audience,
        DateTimeOffset? expiresAt,
        IReadOnlyCollection<string>? roles,
        string tenantId,
        bool includeTenantClaim,
        string keyId = KeyId)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset expires = expiresAt ?? now.AddMinutes(10);

        var header = new Dictionary<string, object?>
        {
            ["alg"] = "RS256",
            ["kid"] = keyId,
            ["typ"] = "JWT"
        };
        var payload = new Dictionary<string, object?>
        {
            ["iss"] = issuer,
            ["aud"] = audience,
            ["sub"] = "local-petshop-user",
            ["preferred_username"] = "local.petshop.user",
            ["iat"] = now.ToUnixTimeSeconds(),
            ["nbf"] = now.AddMinutes(-1).ToUnixTimeSeconds(),
            ["exp"] = expires.ToUnixTimeSeconds(),
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
        byte[] signature = signingKey.SignData(
            Encoding.UTF8.GetBytes(signingInput),
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        return string.Join('.', signingInput, Base64UrlEncode(signature));
    }

    private static string CreateTokenWithWrongSignature()
    {
        using RSA invalidSignatureKey = RSA.Create(2048);

        return CreateTokenCore(
            invalidSignatureKey,
            Issuer,
            Audience,
            DateTimeOffset.UtcNow.AddMinutes(10),
            [RequiredRole],
            TenantA,
            includeTenantClaim: true,
            KeyId);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private sealed record DiagnosticsResponse(
        string Service,
        string Environment,
        string TenantId,
        string? CorrelationId);

    public enum InvalidTokenKind
    {
        Malformed,
        Expired,
        WrongIssuer,
        WrongAudience,
        WrongSignature
    }
}
