using System.Reflection;

using PetShop.Api;
using PetShop.Observability.AspNetCore.Extensions;
using PetShop.Observability.Context;
using PetShop.Tutores;

namespace PetShop.ArchitectureTests;

public sealed class TutoresModuleBoundaryTests
{
    private const BindingFlags TypeMemberFlags =
        BindingFlags.Public |
        BindingFlags.NonPublic |
        BindingFlags.Instance |
        BindingFlags.Static |
        BindingFlags.DeclaredOnly;

    [Fact]
    public void Api_ReferencesTutoresModule()
    {
        Assembly api = typeof(ApiAssemblyMarker).Assembly;
        string? tutoresAssemblyName = typeof(ModuloTutoresServiceCollectionExtensions).Assembly.GetName().Name;

        Assert.Contains(
            api.GetReferencedAssemblies(),
            reference => reference.Name == tutoresAssemblyName);
    }

    [Fact]
    public void Tutores_PublicSurfaceIsMinimal()
    {
        Assembly tutores = typeof(ModuloTutoresServiceCollectionExtensions).Assembly;

        string[] publicTypes = tutores
            .GetExportedTypes()
            .Select(type => type.FullName)
            .Order(StringComparer.Ordinal)
            .ToArray()!;

        Assert.Equal(
            [
                "PetShop.Tutores.Infrastructure.ModuloTutoresPersistenceExtensions",
                "PetShop.Tutores.ModuloTutoresEndpointRouteBuilderExtensions",
                "PetShop.Tutores.ModuloTutoresServiceCollectionExtensions"
            ],
            publicTypes);
    }

    [Fact]
    public void TutoresDomain_DoesNotDependOnInfrastructureOrApi()
    {
        AssertNamespaceDoesNotReferenceNamespaces(
            "PetShop.Tutores.Domain",
            [
                "PetShop.Tutores.Infrastructure",
                "PetShop.Tutores.Api"
            ]);
    }

    [Fact]
    public void TutoresApplication_DoesNotDependOnApi()
    {
        AssertNamespaceDoesNotReferenceNamespaces(
            "PetShop.Tutores.Application",
            ["PetShop.Tutores.Api"]);
    }

    [Fact]
    public void TutoresApplication_DoesNotExposeIQueryableContracts()
    {
        Assembly tutores = typeof(ModuloTutoresServiceCollectionExtensions).Assembly;

        string[] violations = tutores
            .GetTypes()
            .Where(type => IsInNamespace(type, "PetShop.Tutores.Application"))
            .SelectMany(type => type.GetMethods(TypeMemberFlags)
                .SelectMany(method => MethodContractTypes(method)
                    .Where(IsQueryableType)
                    .Select(contractType => $"{type.FullName}.{method.Name} -> {contractType.FullName}")))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void OtherProductionAssemblies_DoNotAccessTutoresInternals()
    {
        Assembly tutores = typeof(ModuloTutoresServiceCollectionExtensions).Assembly;
        HashSet<Type> tutoresInternalTypes = tutores
            .GetTypes()
            .Where(type => !type.IsVisible)
            .ToHashSet();

        string[] violations = ProductionAssemblies()
            .Where(assembly => assembly != tutores)
            .SelectMany(assembly => assembly.GetTypes())
            .SelectMany(type => ReferencedTypes(type)
                .Where(tutoresInternalTypes.Contains)
                .Select(referencedType => $"{type.FullName} -> {referencedType.FullName}"))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void Tutores_DoesNotAccessInternalsFromOtherProductionAssemblies()
    {
        Assembly tutores = typeof(ModuloTutoresServiceCollectionExtensions).Assembly;
        HashSet<Type> otherInternalTypes = ProductionAssemblies()
            .Where(assembly => assembly != tutores)
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsVisible)
            .ToHashSet();

        string[] violations = tutores
            .GetTypes()
            .SelectMany(type => ReferencedTypes(type)
                .Where(otherInternalTypes.Contains)
                .Select(referencedType => $"{type.FullName} -> {referencedType.FullName}"))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(violations);
    }

    private static void AssertNamespaceDoesNotReferenceNamespaces(
        string sourceNamespace,
        IReadOnlyCollection<string> forbiddenNamespaces)
    {
        Assembly tutores = typeof(ModuloTutoresServiceCollectionExtensions).Assembly;

        string[] violations = tutores
            .GetTypes()
            .Where(type => IsInNamespace(type, sourceNamespace))
            .SelectMany(type => ReferencedTypes(type)
                .Where(referencedType => forbiddenNamespaces.Any(forbidden => IsInNamespace(referencedType, forbidden)))
                .Select(referencedType => $"{type.FullName} -> {referencedType.FullName}"))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(violations);
    }

    private static Assembly[] ProductionAssemblies() =>
        [
            typeof(ApiAssemblyMarker).Assembly,
            typeof(ModuloTutoresServiceCollectionExtensions).Assembly,
            typeof(IExecutionContextAccessor).Assembly,
            typeof(ObservabilityApplicationBuilderExtensions).Assembly
        ];

    private static bool IsInNamespace(Type type, string namespacePrefix) =>
        type.Namespace == namespacePrefix ||
        type.Namespace?.StartsWith(namespacePrefix + ".", StringComparison.Ordinal) == true;

    private static HashSet<Type> ReferencedTypes(Type type)
    {
        var referencedTypes = new HashSet<Type>();

        AddType(type.BaseType, referencedTypes);

        foreach (Type interfaceType in type.GetInterfaces())
        {
            AddType(interfaceType, referencedTypes);
        }

        foreach (FieldInfo field in type.GetFields(TypeMemberFlags))
        {
            AddType(field.FieldType, referencedTypes);
        }

        foreach (PropertyInfo property in type.GetProperties(TypeMemberFlags))
        {
            AddType(property.PropertyType, referencedTypes);
        }

        foreach (EventInfo eventInfo in type.GetEvents(TypeMemberFlags))
        {
            AddType(eventInfo.EventHandlerType, referencedTypes);
        }

        foreach (ConstructorInfo constructor in type.GetConstructors(TypeMemberFlags))
        {
            foreach (ParameterInfo parameter in constructor.GetParameters())
            {
                AddType(parameter.ParameterType, referencedTypes);
            }
        }

        foreach (MethodInfo method in type.GetMethods(TypeMemberFlags))
        {
            AddType(method.ReturnType, referencedTypes);

            foreach (ParameterInfo parameter in method.GetParameters())
            {
                AddType(parameter.ParameterType, referencedTypes);
            }
        }

        return referencedTypes;
    }

    private static IEnumerable<Type> MethodContractTypes(MethodInfo method)
    {
        yield return method.ReturnType;

        foreach (ParameterInfo parameter in method.GetParameters())
        {
            yield return parameter.ParameterType;
        }
    }

    private static bool IsQueryableType(Type type)
    {
        if (type == typeof(IQueryable))
        {
            return true;
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IQueryable<>))
        {
            return true;
        }

        return type.GetInterfaces().Any(IsQueryableType);
    }

    private static void AddType(Type? type, ISet<Type> referencedTypes)
    {
        if (type is null)
        {
            return;
        }

        if (type.HasElementType)
        {
            AddType(type.GetElementType(), referencedTypes);
            return;
        }

        _ = referencedTypes.Add(type);

        if (!type.IsGenericType)
        {
            return;
        }

        foreach (Type argument in type.GetGenericArguments())
        {
            AddType(argument, referencedTypes);
        }
    }
}
