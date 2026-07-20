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
            project.Descendants("ProjectReference"),
            reference => reference.Attribute("Include")?.Value.Replace('\\', '/') ==
                "../../Apps/PetShop.Api/PetShop.Api.csproj");
    }

    [Fact]
    public async Task AppHost_ComposesOnlyApiProject()
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
        Assert.DoesNotContain("Postgres", program, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Keycloak", program, StringComparison.OrdinalIgnoreCase);
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
