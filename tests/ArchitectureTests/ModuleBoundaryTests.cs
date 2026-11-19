using ArchUnitNET.xUnitV3;

namespace TeamGym.Arena.ArchitectureTests;

/// <summary>Applies the <see cref="ModuleRules"/> to every module of the solution.</summary>
public sealed class ModuleBoundaryTests
{
    public static TheoryData<string> Modules => [.. SolutionArchitecture.Modules];

    public static TheoryData<string> ModuleAssemblies =>
    [
        .. SolutionArchitecture.AssemblyNames.Where(name =>
            name.StartsWith(SolutionArchitecture.ModulesNamespace + ".", StringComparison.Ordinal)),
    ];

    [Fact]
    public void EveryModuleIsDiscovered()
    {
        // Guards against a discovery bug that would make every other test in this class pass without checking
        // anything. The Api host references every module, so they are all next to the tests.
        Assert.Superset(
            new HashSet<string> { "Applications", "Competitions", "Organisations", "Scheduling", "Users" },
            SolutionArchitecture.Modules.ToHashSet());
    }

    [Theory]
    [MemberData(nameof(Modules))]
    public void ImplementationDoesNotDependOnOtherModules(string module) =>
        ModuleRules.ImplementationDoesNotDependOnOtherModules(SolutionArchitecture.Layout, module)
            .Check(SolutionArchitecture.Architecture);

    [Theory]
    [MemberData(nameof(Modules))]
    public void ContractsDoNotDependOnModuleImplementations(string module) =>
        ModuleRules.ContractsDoNotDependOnModuleImplementations(SolutionArchitecture.Layout, module)
            .Check(SolutionArchitecture.Architecture);

    [Theory]
    [MemberData(nameof(Modules))]
    public void DomainDoesNotDependOnInfrastructure(string module) =>
        ModuleRules.DomainDoesNotDependOnInfrastructure(SolutionArchitecture.Layout, module)
            .Check(SolutionArchitecture.Architecture);

    [Theory]
    [MemberData(nameof(Modules))]
    public void ModuleDoesNotDependOnHosts(string module) =>
        ModuleRules.ModuleDoesNotDependOnHosts(
                SolutionArchitecture.Layout,
                module,
                SolutionArchitecture.IsHostType,
                "are in a host assembly")
            .Check(SolutionArchitecture.Architecture);

    [Theory]
    [MemberData(nameof(Modules))]
    public void ModuleDoesNotDependOnCloudSdks(string module) =>
        ModuleRules.ModuleDoesNotDependOnCloudSdks(SolutionArchitecture.Layout, module)
            .Check(SolutionArchitecture.Architecture);

    [Theory]
    [MemberData(nameof(Modules))]
    public void OnlyRegistrationIsPublic(string module) =>
        ModuleRules.OnlyRegistrationIsPublic(SolutionArchitecture.Layout, module)
            .Check(SolutionArchitecture.Architecture);

    [Theory]
    [MemberData(nameof(ModuleAssemblies))]
    public void TypesResideInTheNamespaceOfTheirAssembly(string assemblyName) =>
        ModuleRules.TypesResideInTheNamespaceOfTheirAssembly(assemblyName)
            .Check(SolutionArchitecture.Architecture);
}
