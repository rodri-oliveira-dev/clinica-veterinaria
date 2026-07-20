using System.Reflection;
using System.Xml.Linq;

using PetShop.Api;
using PetShop.Observability.AspNetCore.Extensions;
using PetShop.Observability.Context;

namespace PetShop.ArchitectureTests;

public sealed class ProjectReferenceTests
{
    [Fact]
    public void ObservabilityCore_DoesNotDependOnAspNetCore()
    {
        Assembly observability = typeof(IExecutionContextAccessor).Assembly;

        Assert.DoesNotContain(
            observability.GetReferencedAssemblies(),
            reference => reference.Name?.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal) == true);
    }

    [Fact]
    public void Api_UsesAspNetCoreObservabilityAdapter()
    {
        Assembly api = typeof(ApiAssemblyMarker).Assembly;

        Assert.Contains(
            api.GetReferencedAssemblies(),
            reference => reference.Name == typeof(ObservabilityApplicationBuilderExtensions).Assembly.GetName().Name);
    }

    [Fact]
    public void Api_DoesNotReferenceAppHost()
    {
        Assembly api = typeof(ApiAssemblyMarker).Assembly;

        Assert.DoesNotContain(
            api.GetReferencedAssemblies(),
            reference => reference.Name == "PetShop.AppHost");
    }

    [Fact]
    public void ProductionProjects_DoNotReferenceTestProjects()
    {
        IReadOnlyDictionary<string, ProjectNode> projects = LoadProjectGraph();

        foreach (ProjectNode project in projects.Values.Where(project => project.RelativePath.StartsWith("src/", StringComparison.Ordinal)))
        {
            Assert.DoesNotContain(
                project.ProjectReferences,
                reference => projects[reference].RelativePath.StartsWith("tests/", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void ProjectReferences_DoNotHaveCycles()
    {
        IReadOnlyDictionary<string, ProjectNode> projects = LoadProjectGraph();

        foreach (string projectPath in projects.Keys)
        {
            Assert.Empty(FindCycle(projectPath, projects));
        }
    }

    private static Dictionary<string, ProjectNode> LoadProjectGraph()
    {
        string repositoryRoot = FindRepositoryRoot();
        string[] projectFiles = Directory.GetFiles(repositoryRoot, "*.csproj", SearchOption.AllDirectories);
        var projects = projectFiles.ToDictionary(
            path => NormalizePath(path),
            path => new ProjectNode(
                RelativePath(repositoryRoot, path),
                XDocument.Load(path)
                    .Descendants("ProjectReference")
                    .Select(reference => reference.Attribute("Include")?.Value)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => ResolveProjectReferencePath(path, value!))
                    .ToArray()));

        return projects;
    }

    private static string[] FindCycle(
        string projectPath,
        IReadOnlyDictionary<string, ProjectNode> projects)
    {
        var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var stack = new Stack<string>();

        return Visit(projectPath) ? stack.Reverse().ToArray() : [];

        bool Visit(string current)
        {
            if (!projects.TryGetValue(current, out ProjectNode? project))
            {
                return false;
            }

            if (visiting.Contains(current))
            {
                stack.Push(current);
                return true;
            }

            if (!visited.Add(current))
            {
                return false;
            }

            visiting.Add(current);
            stack.Push(current);

            foreach (string reference in project.ProjectReferences)
            {
                if (Visit(reference))
                {
                    return true;
                }
            }

            visiting.Remove(current);
            stack.Pop();
            return false;
        }
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

    private static string NormalizePath(string path)
    {
        return Path.GetFullPath(path)
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/');
    }

    private static string ResolveProjectReferencePath(string projectFilePath, string referencePath)
    {
        string platformReferencePath = referencePath
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);

        return NormalizePath(Path.GetFullPath(platformReferencePath, Path.GetDirectoryName(projectFilePath)!));
    }

    private static string RelativePath(string repositoryRoot, string path)
    {
        return Path.GetRelativePath(repositoryRoot, path)
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/');
    }

    private sealed record ProjectNode(
        string RelativePath,
        IReadOnlyCollection<string> ProjectReferences);
}
