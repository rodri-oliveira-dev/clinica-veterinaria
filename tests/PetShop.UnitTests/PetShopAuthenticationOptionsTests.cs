using Microsoft.Extensions.Hosting;

using PetShop.Api.Authentication;

namespace PetShop.UnitTests;

public sealed class PetShopAuthenticationOptionsTests
{
    [Fact]
    public void Validate_WithMetadataAddress_KeepsAuthorityAndMetadataSeparate()
    {
        var options = new PetShopAuthenticationOptions
        {
            Authority = "https://keycloak.public.example/realms/petshop-local/",
            MetadataAddress = "https://keycloak.private.example/realms/petshop-local/.well-known/openid-configuration",
            Audience = "petshop-api",
            RequiredRole = "petshop.access",
            RequireHttpsMetadata = true
        };

        options.Validate(new TestHostEnvironment());

        Assert.Equal("https://keycloak.public.example/realms/petshop-local", options.ResolvedAuthority);
        Assert.Equal(
            "https://keycloak.private.example/realms/petshop-local/.well-known/openid-configuration",
            options.ResolvedMetadataAddress);
    }

    [Fact]
    public void Validate_WithHttpMetadataAddressAndHttpsMetadataRequired_Throws()
    {
        var options = new PetShopAuthenticationOptions
        {
            Authority = "https://keycloak.public.example/realms/petshop-local",
            MetadataAddress = "http://keycloak:8080/realms/petshop-local/.well-known/openid-configuration",
            Audience = "petshop-api",
            RequiredRole = "petshop.access",
            RequireHttpsMetadata = true
        };

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => options.Validate(new TestHostEnvironment()));

        Assert.Contains("metadata address must use HTTPS", exception.Message, StringComparison.Ordinal);
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;

        public string ApplicationName { get; set; } = "PetShop.Api";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
