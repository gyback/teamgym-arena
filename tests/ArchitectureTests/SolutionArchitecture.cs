using ArchUnitNET.Domain;
using ArchUnitNET.Loader;

namespace TeamGym.Arena.ArchitectureTests;

/// <summary>
/// The production assemblies of the solution, loaded once for every test. The project reference to the Api host
/// copies every module and its contracts next to the tests, so new modules are picked up without changes here.
/// </summary>
internal static class SolutionArchitecture
{
    private const string AssemblyPrefix = "TeamGym.Arena.";

    /// <summary>The namespace and assembly name prefix that every module lives under.</summary>
    public const string ModulesNamespace = "TeamGym.Arena.Modules";

    /// <summary>The assemblies that compose the modules and that no module may depend on.</summary>
    public static readonly IReadOnlySet<string> HostAssemblies = new HashSet<string>(StringComparer.Ordinal)
    {
        "TeamGym.Arena.Api",
        "TeamGym.Arena.AppHost",
        "TeamGym.Arena.MigrationService",
        "TeamGym.Arena.ServiceDefaults",
    };

    public static readonly ModuleLayout Layout = new(ModulesNamespace);

    /// <summary>The simple names of every loaded production assembly.</summary>
    public static readonly IReadOnlyList<string> AssemblyNames = FindAssemblyNames();

    /// <summary>The names of every module, such as <c>Users</c>, taken from the module implementation assemblies.</summary>
    public static readonly IReadOnlyList<string> Modules =
    [
        .. AssemblyNames
            .Where(name => name.StartsWith(ModulesNamespace + ".", StringComparison.Ordinal))
            .Select(name => name[(ModulesNamespace.Length + 1)..])
            .Where(module => !module.Contains('.', StringComparison.Ordinal))
            .Order(StringComparer.Ordinal),
    ];

    public static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies([.. AssemblyNames.Select(name => System.Reflection.Assembly.Load(name))])
        .Build();

    /// <summary>Whether <paramref name="type"/> is declared in one of the <see cref="HostAssemblies"/>.</summary>
    public static bool IsHostType(IType type) => HostAssemblies.Contains(ModuleRules.AssemblyName(type));

    private static string[] FindAssemblyNames()
    {
        var testAssembly = typeof(SolutionArchitecture).Assembly.GetName().Name;

        return
        [
            .. Directory.EnumerateFiles(AppContext.BaseDirectory, AssemblyPrefix + "*.dll")
                .Select(Path.GetFileNameWithoutExtension)
                .OfType<string>()
                .Where(name => name != testAssembly)
                .Order(StringComparer.Ordinal),
        ];
    }
}
