using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace TeamGym.Arena.ArchitectureTests;

/// <summary>
/// The module boundary rules of ADR-0004, ADR-0007 and ADR-0009, built for one module of a
/// <see cref="ModuleLayout"/>. The rules are parameterised so that <c>RuleFixtureTests</c> can prove that each
/// one reports a planted violation, and not only that the current code passes it.
/// </summary>
internal static class ModuleRules
{
    /// <summary>
    /// Namespaces that a pure domain model never references: EF Core and its PostgreSQL provider, ASP.NET Core,
    /// hosting and DI infrastructure, HTTP clients and Azure SDKs (ADR-0004, ADR-0007).
    /// </summary>
    public static readonly IReadOnlyList<string> InfrastructureNamespaces =
    [
        "Azure",
        "Microsoft.AspNetCore",
        "Microsoft.EntityFrameworkCore",
        "Microsoft.Extensions",
        "Npgsql",
        "System.Net.Http",
    ];

    /// <summary>Namespaces of cloud SDKs, which only the host may reference (ADR-0009).</summary>
    public static readonly IReadOnlyList<string> CloudSdkNamespaces = ["Azure"];

    /// <summary>
    /// A module reaches another module only through that module's <c>.Contracts</c>, never through its
    /// implementation (ADR-0004).
    /// </summary>
    public static IArchRule ImplementationDoesNotDependOnOtherModules(ModuleLayout layout, string module) =>
        Types().That().FollowCustomPredicate(
                type => layout.IsImplementationOf(type, module),
                $"are in the implementation of the {module} module")
            .Should().NotDependOnAnyTypesThat().FollowCustomPredicate(
                type => layout.IsImplementationOfAnotherModule(type, module),
                "are in the implementation of another module")
            .Because("modules reach each other only through <Module>.Contracts (ADR-0004)")
            .WithoutRequiringPositiveResults();

    /// <summary>
    /// Contracts are what other modules compile against, so they never expose a module implementation, not even
    /// their own (ADR-0004).
    /// </summary>
    public static IArchRule ContractsDoNotDependOnModuleImplementations(ModuleLayout layout, string module) =>
        Types().That().FollowCustomPredicate(
                type => layout.IsContractsOf(type, module),
                $"are in the contracts of the {module} module")
            .Should().NotDependOnAnyTypesThat().FollowCustomPredicate(
                layout.IsImplementationOfAnyModule,
                "are in the implementation of a module")
            .Because("contracts hold only DTOs, integration events and query interfaces (ADR-0004)")
            .WithoutRequiringPositiveResults();

    /// <summary>
    /// The domain model is pure C#: no EF Core, HTTP or infrastructure references (ADR-0004, ADR-0007).
    /// </summary>
    public static IArchRule DomainDoesNotDependOnInfrastructure(ModuleLayout layout, string module)
    {
        ArgumentNullException.ThrowIfNull(layout);

        var domainNamespace = layout.DomainNamespace(module);

        return Types().That().FollowCustomPredicate(
                type => ModuleLayout.IsInNamespaceOrBelow(type, domainNamespace),
                $"are in {domainNamespace}")
            .Should().NotDependOnAnyTypesThat().FollowCustomPredicate(
                type => IsInAnyNamespace(type, InfrastructureNamespaces),
                $"are in {string.Join(", ", InfrastructureNamespaces)}")
            .Because("the domain model is persistence-ignorant pure C# (ADR-0004, ADR-0007)")
            .WithoutRequiringPositiveResults();
    }

    /// <summary>Modules never depend on a host; the host composes the modules (ADR-0004).</summary>
    public static IArchRule ModuleDoesNotDependOnHosts(
        ModuleLayout layout,
        string module,
        Func<IType, bool> isHostType,
        string hostDescription) =>
        Types().That().FollowCustomPredicate(
                type => layout.IsPartOf(type, module),
                $"are in the {module} module")
            .Should().NotDependOnAnyTypesThat().FollowCustomPredicate(isHostType, hostDescription)
            .Because("the host composes the modules, never the other way round (ADR-0004)")
            .WithoutRequiringPositiveResults();

    /// <summary>
    /// Cloud SDKs and <c>DefaultAzureCredential</c> appear only in host infrastructure registrations (ADR-0009).
    /// </summary>
    public static IArchRule ModuleDoesNotDependOnCloudSdks(ModuleLayout layout, string module) =>
        Types().That().FollowCustomPredicate(
                type => layout.IsPartOf(type, module),
                $"are in the {module} module")
            .Should().NotDependOnAnyTypesThat().FollowCustomPredicate(
                type => IsInAnyNamespace(type, CloudSdkNamespaces),
                $"are in {string.Join(", ", CloudSdkNamespaces)}")
            .Because("only the host may reference cloud SDKs (ADR-0009)")
            .WithoutRequiringPositiveResults();

    /// <summary>
    /// Everything in a module implementation is internal except its <c>&lt;Module&gt;Module</c> registration
    /// class, so feature handlers, endpoints, domain types and the <c>DbContext</c> stay inside the module
    /// (ADR-0004, ADR-0007). Nested types are left out, because a public type nested in an internal type is not
    /// visible outside the assembly.
    /// </summary>
    public static IArchRule OnlyRegistrationIsPublic(ModuleLayout layout, string module) =>
        Types().That().FollowCustomPredicate(
                type => layout.IsImplementationOf(type, module),
                $"are in the implementation of the {module} module")
            .And().ArePublic()
            .And().AreNotNested()
            .Should().HaveName($"{module}Module")
            .Because("module types are internal by default and only the registration class is public (ADR-0004)")
            .WithoutRequiringPositiveResults();

    /// <summary>
    /// Every type in an assembly lives in the namespace named after the assembly, or below it. This keeps
    /// <see cref="ModuleLayout"/>'s namespace checks equal to assembly checks.
    /// </summary>
    public static IArchRule TypesResideInTheNamespaceOfTheirAssembly(string assemblyName) =>
        Types().That().FollowCustomPredicate(
                type => AssemblyName(type) == assemblyName && !type.IsCompilerGenerated,
                $"are in the {assemblyName} assembly")
            .Should().ResideInNamespaceOrBelow(assemblyName)
            .Because("a module's namespaces match its assembly name (ADR-0004)")
            .WithoutRequiringPositiveResults();

    /// <summary>Gets the simple name of the assembly that <paramref name="type"/> is declared in.</summary>
    public static string AssemblyName(IType type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var name = type.Assembly.Name;
        var separator = name.IndexOf(',', StringComparison.Ordinal);

        return separator < 0 ? name : name[..separator];
    }

    private static bool IsInAnyNamespace(IType type, IEnumerable<string> namespaces) =>
        namespaces.Any(ns => ModuleLayout.IsInNamespaceOrBelow(type, ns));
}
