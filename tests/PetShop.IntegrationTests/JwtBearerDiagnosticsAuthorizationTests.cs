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
    private const string TenantId = "11111111-1111-4111-8111-111111111111";
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

        using HttpResponseMessage response = await client.GetAsync(
            "/diagnostics",
            TestContext.Current.CancellationToken);

        AssertStatusCode(HttpStatusCode.Unauthorized, response);
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
    }

    [Fact]
    public async Task Diagnostics_WithValidAuthorizedToken_ReturnsOkWithoutClaimsDump()
    {
        using HttpClient client = _factory.CreateClient();
        string token = CreateToken(roles: [RequiredRole]);
        const string correlationId = "75db3340-c2e7-4dd5-886b-6c39530bd88c";

        using HttpResponseMessage response = await SendDiagnosticsAsync(client, token, correlationId);

        AssertStatusCode(HttpStatusCode.OK, response);
        DiagnosticsResponse? body = await response.Content.ReadFromJsonAsync<DiagnosticsResponse>(
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal("PetShop.Api", body.Service);
        Assert.Equal("Development", body.Environment);
        Assert.Equal(correlationId, body.CorrelationId);
    }

    public void Dispose()
    {
        _factory.Dispose();
        _rsa.Dispose();
    }

    private static async Task<HttpResponseMessage> SendDiagnosticsAsync(
        HttpClient client,
        string token,
        string? correlationId = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/diagnostics");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (correlationId is not null)
        {
            request.Headers.TryAddWithoutValidation("X-Correlation-Id", correlationId);
        }

        return await client.SendAsync(request, TestContext.Current.CancellationToken);
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
        IReadOnlyCollection<string>? roles = null)
    {
        return CreateTokenCore(_rsa, issuer, audience, expiresAt, roles, KeyId);
    }

    private static string CreateTokenCore(
        RSA signingKey,
        string issuer,
        string audience,
        DateTimeOffset? expiresAt,
        IReadOnlyCollection<string>? roles,
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
            ["tenant_id"] = TenantId,
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
