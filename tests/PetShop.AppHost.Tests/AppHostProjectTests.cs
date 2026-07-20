using System.Text.Json;
using System.Xml.Linq;

namespace PetShop.AppHost.Tests;

public sealed class AppHostProjectTests
{
    private const string LocalRealmName = "petshop-local";
    private const string LocalApiClientId = "petshop-api";
    private const string LocalAccessRole = "petshop.access";
    private const string LocalUsername = "local.petshop.user";
    private const string LocalTenantId = "11111111-1111-4111-8111-111111111111";

    [Fact]
    public void AppHost_IsAspireHostAndReferencesApi()
    {
        string repositoryRoot = FindRepositoryRoot();
        string projectPath = Path.Combine(
            repositoryRoot,
            "src",
            "AppHost",
            "PetShop.AppHost",
            "PetShop.AppHost.csproj");

        XDocument project = XDocument.Load(projectPath);

        Assert.Equal("Aspire.AppHost.Sdk/13.4.6", project.Root?.Attribute("Sdk")?.Value);
        Assert.Contains(
            project.Descendants("PackageReference"),
            reference => reference.Attribute("Include")?.Value == "Aspire.Hosting.AppHost");
        Assert.Contains(
            project.Descendants("PackageReference"),
            reference => reference.Attribute("Include")?.Value == "Aspire.Hosting.PostgreSQL");
        Assert.Contains(
            project.Descendants("PackageReference"),
            reference => reference.Attribute("Include")?.Value == "Aspire.Hosting.Keycloak");
        Assert.Contains(
            project.Descendants("ProjectReference"),
            reference => reference.Attribute("Include")?.Value.Replace('\\', '/') ==
                "../../Apps/PetShop.Api/PetShop.Api.csproj");
    }

    [Fact]
    public async Task AppHost_ComposesApiPostgresAndKeycloakForLocalDevelopment()
    {
        string repositoryRoot = FindRepositoryRoot();
        string programPath = Path.Combine(
            repositoryRoot,
            "src",
            "AppHost",
            "PetShop.AppHost",
            "Program.cs");

        string program = await File.ReadAllTextAsync(programPath, TestContext.Current.CancellationToken);

        Assert.Contains("AddProject<Projects.PetShop_Api>(\"petshop-api\")", program, StringComparison.Ordinal);
        Assert.Contains("AddPostgres(\"postgres\")", program, StringComparison.Ordinal);
        Assert.Contains("AddDatabase(\"petshop\")", program, StringComparison.Ordinal);
        Assert.Contains("AddKeycloak(\"keycloak\", 8080)", program, StringComparison.Ordinal);
        Assert.Contains(".WithRealmImport(\"keycloak/realms\")", program, StringComparison.Ordinal);
        Assert.Equal(2, CountOccurrences(program, ".WithDataVolume()"));
        Assert.Contains(".WithReference(petshopDatabase)", program, StringComparison.Ordinal);
        Assert.Contains(".WithReference(keycloak)", program, StringComparison.Ordinal);
        Assert.Contains(".WaitFor(petshopDatabase)", program, StringComparison.Ordinal);
        Assert.Contains(".WaitFor(keycloak)", program, StringComparison.Ordinal);
    }

    [Fact]
    public async Task KeycloakRealmImport_DefinesLocalRealmClientAudienceRoleUserAndTenant()
    {
        string repositoryRoot = FindRepositoryRoot();
        string realmPath = Path.Combine(
            repositoryRoot,
            "src",
            "AppHost",
            "PetShop.AppHost",
            "keycloak",
            "realms",
            "petshop-local-realm.json");

        await using FileStream stream = File.OpenRead(realmPath);
        using JsonDocument realm = await JsonDocument.ParseAsync(stream, cancellationToken: TestContext.Current.CancellationToken);
        JsonElement root = realm.RootElement;

        Assert.Equal(LocalRealmName, root.GetProperty("realm").GetString());
        Assert.True(root.GetProperty("enabled").GetBoolean());

        JsonElement client = Assert.Single(
            root.GetProperty("clients").EnumerateArray(),
            element => element.GetProperty("clientId").GetString() == LocalApiClientId);

        Assert.True(client.GetProperty("enabled").GetBoolean());
        Assert.True(client.GetProperty("publicClient").GetBoolean());
        Assert.True(client.GetProperty("directAccessGrantsEnabled").GetBoolean());
        Assert.False(client.GetProperty("standardFlowEnabled").GetBoolean());
        Assert.False(client.GetProperty("implicitFlowEnabled").GetBoolean());
        Assert.Contains(
            client.GetProperty("protocolMappers").EnumerateArray(),
            mapper =>
                mapper.GetProperty("protocolMapper").GetString() == "oidc-audience-mapper" &&
                mapper.GetProperty("config").GetProperty("included.client.audience").GetString() == LocalApiClientId &&
                mapper.GetProperty("config").GetProperty("access.token.claim").GetString() == "true");
        Assert.Contains(
            client.GetProperty("protocolMappers").EnumerateArray(),
            mapper =>
                mapper.GetProperty("protocolMapper").GetString() == "oidc-usermodel-attribute-mapper" &&
                mapper.GetProperty("config").GetProperty("user.attribute").GetString() == "tenant_id" &&
                mapper.GetProperty("config").GetProperty("claim.name").GetString() == "tenant_id" &&
                mapper.GetProperty("config").GetProperty("access.token.claim").GetString() == "true");

        JsonElement apiRoles = root.GetProperty("roles").GetProperty("client").GetProperty(LocalApiClientId);
        Assert.Contains(apiRoles.EnumerateArray(), role => role.GetProperty("name").GetString() == LocalAccessRole);

        JsonElement user = Assert.Single(
            root.GetProperty("users").EnumerateArray(),
            element => element.GetProperty("username").GetString() == LocalUsername);

        Assert.True(user.GetProperty("enabled").GetBoolean());
        Assert.Equal(
            LocalTenantId,
            Assert.Single(user.GetProperty("attributes").GetProperty("tenant_id").EnumerateArray()).GetString());
        Assert.Contains(
            user.GetProperty("clientRoles").GetProperty(LocalApiClientId).EnumerateArray(),
            role => role.GetString() == LocalAccessRole);

        Assert.True(Guid.TryParse(LocalTenantId, out _));
    }

    private static int CountOccurrences(string value, string expected)
    {
        int count = 0;
        int index = 0;

        while ((index = value.IndexOf(expected, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += expected.Length;
        }

        return count;
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "ClinicaVeterinaria.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}
