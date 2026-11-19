using TeamGym.Arena.ArchitectureTests.Fixtures.Modules.Alpha.Domain;

namespace TeamGym.Arena.ArchitectureTests.Fixtures.Modules.Alpha.Contracts;

/// <summary>Allowed: contracts depend only on the base class library.</summary>
public interface IAlphaQueries
{
    int CountTeams();
}

/// <summary>Violation: contracts expose the implementation of their own module.</summary>
internal interface ILeakyContract
{
    IPureAggregate Aggregate { get; }
}
