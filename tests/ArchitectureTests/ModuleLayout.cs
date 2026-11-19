using ArchUnitNET.Domain;

namespace TeamGym.Arena.ArchitectureTests;

/// <summary>
/// Classifies types by the module they belong to. A module named <c>M</c> under the modules namespace <c>R</c>
/// has its implementation in namespace <c>R.M</c> and below, and its contracts in <c>R.M.Contracts</c> and below
/// (ADR-0004). <see cref="ModuleRules.TypesResideInTheNamespaceOfTheirAssembly"/> keeps namespaces and
/// assemblies aligned, so a namespace check is also an assembly check.
/// </summary>
/// <param name="ModulesNamespace">The namespace that every module lives under, such as <c>TeamGym.Arena.Modules</c>.</param>
internal sealed record ModuleLayout(string ModulesNamespace)
{
    private const string ContractsSegment = "Contracts";

    /// <summary>Gets the namespace of the implementation of <paramref name="module"/>.</summary>
    public string ImplementationNamespace(string module) => $"{ModulesNamespace}.{module}";

    /// <summary>Gets the namespace of the contracts of <paramref name="module"/>.</summary>
    public string ContractsNamespace(string module) => $"{ImplementationNamespace(module)}.{ContractsSegment}";

    /// <summary>Gets the namespace of the domain model inside the implementation of <paramref name="module"/>.</summary>
    public string DomainNamespace(string module) => $"{ImplementationNamespace(module)}.Domain";

    /// <summary>Whether <paramref name="type"/> belongs to the implementation of <paramref name="module"/>.</summary>
    public bool IsImplementationOf(IType type, string module) =>
        IsInNamespaceOrBelow(type, ImplementationNamespace(module)) && !IsContractsOf(type, module);

    /// <summary>Whether <paramref name="type"/> belongs to the contracts of <paramref name="module"/>.</summary>
    public bool IsContractsOf(IType type, string module) =>
        IsInNamespaceOrBelow(type, ContractsNamespace(module));

    /// <summary>Whether <paramref name="type"/> belongs to the implementation or the contracts of <paramref name="module"/>.</summary>
    public bool IsPartOf(IType type, string module) =>
        IsInNamespaceOrBelow(type, ImplementationNamespace(module));

    /// <summary>Whether <paramref name="type"/> belongs to the implementation of any module other than <paramref name="module"/>.</summary>
    public bool IsImplementationOfAnotherModule(IType type, string module) =>
        ModuleOf(type) is { } owner && owner != module && IsImplementationOf(type, owner);

    /// <summary>Whether <paramref name="type"/> belongs to the implementation of any module.</summary>
    public bool IsImplementationOfAnyModule(IType type) =>
        ModuleOf(type) is { } owner && IsImplementationOf(type, owner);

    /// <summary>Whether <paramref name="type"/> resides in <paramref name="ns"/> or one of its child namespaces.</summary>
    public static bool IsInNamespaceOrBelow(IType type, string ns)
    {
        ArgumentNullException.ThrowIfNull(type);

        var typeNamespace = type.Namespace.FullName;

        return typeNamespace == ns || typeNamespace.StartsWith(ns + ".", StringComparison.Ordinal);
    }

    private string? ModuleOf(IType type)
    {
        var prefix = ModulesNamespace + ".";
        var typeNamespace = type.Namespace.FullName;

        if (!typeNamespace.StartsWith(prefix, StringComparison.Ordinal))
        {
            return null;
        }

        var rest = typeNamespace[prefix.Length..];
        var separator = rest.IndexOf('.', StringComparison.Ordinal);

        return separator < 0 ? rest : rest[..separator];
    }
}
