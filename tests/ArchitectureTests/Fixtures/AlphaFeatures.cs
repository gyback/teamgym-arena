using TeamGym.Arena.ArchitectureTests.Fixtures.Host;
using TeamGym.Arena.ArchitectureTests.Fixtures.Modules.Beta.Contracts;
using TeamGym.Arena.ArchitectureTests.Fixtures.Modules.Beta.Features;

namespace TeamGym.Arena.ArchitectureTests.Fixtures.Modules.Alpha.Features;

/// <summary>Allowed: another module is reached through its contracts.</summary>
internal interface IUsesBetaContracts
{
    IBetaQueries Queries { get; }
}

/// <summary>Violation: another module is reached through its implementation.</summary>
internal interface IReachesIntoBeta
{
    IBetaHandler Handler { get; }
}

/// <summary>Violation: a module depends on the host.</summary>
internal interface IDependsOnHost
{
    IHostService Host { get; }
}

/// <summary>Violation: a public type in a module implementation that is not the registration class.</summary>
public interface IPublicHandler
{
    void Handle();
}

/// <summary>Allowed: a public type nested in an internal type is not visible outside the assembly.</summary>
internal static class InternalEndpoint
{
    public interface IRequest
    {
        string Name { get; }
    }
}
