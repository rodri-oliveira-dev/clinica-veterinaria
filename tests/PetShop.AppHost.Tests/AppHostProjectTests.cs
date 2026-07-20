using System.Xml.Linq;

namespace PetShop.AppHost.Tests;

public sealed class AppHostProjectTests
{
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
        Assert.Equal(2, CountOccurrences(program, ".WithDataVolume()"));
        Assert.Contains(".WithReference(petshopDatabase)", program, StringComparison.Ordinal);
        Assert.Contains(".WithReference(keycloak)", program, StringComparison.Ordinal);
        Assert.Contains(".WaitFor(petshopDatabase)", program, StringComparison.Ordinal);
        Assert.Contains(".WaitFor(keycloak)", program, StringComparison.Ordinal);
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
