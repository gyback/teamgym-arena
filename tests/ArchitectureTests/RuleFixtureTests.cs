using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using TeamGym.Arena.ArchitectureTests.Fixtures.Host;
using TeamGym.Arena.ArchitectureTests.Fixtures.Modules.Alpha.Contracts;
using TeamGym.Arena.ArchitectureTests.Fixtures.Modules.Alpha.Domain;
using TeamGym.Arena.ArchitectureTests.Fixtures.Modules.Alpha.Features;

namespace TeamGym.Arena.ArchitectureTests;

/// <summary>
/// Runs every <see cref="ModuleRules"/> rule against the fake modules in <c>Fixtures/</c>. The solution has almost
/// no code yet, so a rule that silently matches nothing would still pass <see cref="ModuleBoundaryTests"/>. These
/// tests prove that each rule reports exactly the planted violations and accepts the allowed dependencies.
/// </summary>
public sealed class RuleFixtureTests
{
    private const string Alpha = "Alpha";
    private const string Beta = "Beta";
    private const string FixturesNamespace = "TeamGym.Arena.ArchitectureTests.Fixtures";

    private static readonly ModuleLayout s_layout = new(FixturesNamespace + ".Modules");

    private static readonly Architecture s_architecture = new ArchLoader()
        .LoadNamespacesWithinAssembly(typeof(RuleFixtureTests).Assembly, FixturesNamespace)
        .Build();

    private static readonly Dictionary<string, Func<string, IArchRule>> s_rules = new(StringComparer.Ordinal)
    {
        [nameof(ModuleRules.ImplementationDoesNotDependOnOtherModules)] =
            module => ModuleRules.ImplementationDoesNotDependOnOtherModules(s_layout, module),
        [nameof(ModuleRules.ContractsDoNotDependOnModuleImplementations)] =
            module => ModuleRules.ContractsDoNotDependOnModuleImplementations(s_layout, module),
        [nameof(ModuleRules.DomainDoesNotDependOnInfrastructure)] =
            module => ModuleRules.DomainDoesNotDependOnInfrastructure(s_layout, module),
        [nameof(ModuleRules.ModuleDoesNotDependOnHosts)] =
            module => ModuleRules.ModuleDoesNotDependOnHosts(s_layout, module, IsFixtureHostType, "are in the host"),
        [nameof(ModuleRules.ModuleDoesNotDependOnCloudSdks)] =
            module => ModuleRules.ModuleDoesNotDependOnCloudSdks(s_layout, module),
        [nameof(ModuleRules.OnlyRegistrationIsPublic)] =
            module => ModuleRules.OnlyRegistrationIsPublic(s_layout, module),
    };

    public static TheoryData<string> RuleNames => [.. s_rules.Keys];

    [Fact]
    public void ReportsADependencyOnAnotherModuleImplementation() =>
        AssertViolations(nameof(ModuleRules.ImplementationDoesNotDependOnOtherModules), typeof(IReachesIntoBeta));

    [Fact]
    public void ReportsContractsThatExposeAModuleImplementation() =>
        AssertViolations(nameof(ModuleRules.ContractsDoNotDependOnModuleImplementations), typeof(ILeakyContract));

    [Fact]
    public void ReportsADomainTypeThatDependsOnInfrastructure() =>
        AssertViolations(nameof(ModuleRules.DomainDoesNotDependOnInfrastructure), typeof(IHttpAwareAggregate));

    [Fact]
    public void ReportsAModuleTypeThatDependsOnTheHost() =>
        AssertViolations(nameof(ModuleRules.ModuleDoesNotDependOnHosts), typeof(IDependsOnHost));

    [Fact]
    public void ReportsAPublicTypeOtherThanTheRegistrationClass() =>
        AssertViolations(nameof(ModuleRules.OnlyRegistrationIsPublic), typeof(IPublicHandler));

    [Theory]
    [MemberData(nameof(RuleNames))]
    public void AcceptsAModuleThatFollowsTheRule(string ruleName)
    {
        var results = s_rules[ruleName](Beta).Evaluate(s_architecture).ToList();

        // The rules are built WithoutRequiringPositiveResults, so a rule that selects nothing would pass too.
        Assert.NotEmpty(results);
        Assert.All(results, result => Assert.True(result.Passed, result.Description));
    }

    private static bool IsFixtureHostType(IType type) =>
        ModuleLayout.IsInNamespaceOrBelow(type, typeof(IHostService).Namespace!);

    private static void AssertViolations(string ruleName, params System.Type[] expected)
    {
        var violations = s_rules[ruleName](Alpha).Evaluate(s_architecture)
            .Where(result => !result.Passed)
            .Select(result => ((IType)result.EvaluatedObject).FullName)
            .Order(StringComparer.Ordinal);

        Assert.Equal(expected.Select(type => type.FullName).Order(StringComparer.Ordinal), violations);
    }
}
