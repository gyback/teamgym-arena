using TeamGym.Arena.ArchitectureTests.Fixtures.Modules.Alpha.Contracts;

namespace TeamGym.Arena.ArchitectureTests.Fixtures.Modules.Beta.Contracts;

/// <summary>Allowed: contracts may use other modules' contracts.</summary>
public interface IBetaQueries
{
    IAlphaQueries Alpha { get; }
}
