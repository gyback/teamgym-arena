using TeamGym.Arena.ArchitectureTests.Fixtures.Modules.Alpha.Contracts;
using TeamGym.Arena.ArchitectureTests.Fixtures.Modules.Beta.Domain;

namespace TeamGym.Arena.ArchitectureTests.Fixtures.Modules.Beta.Features;

/// <summary>Allowed: a feature uses its own domain and another module's contracts.</summary>
internal interface IBetaHandler
{
    IBetaAggregate Handle(IAlphaQueries alpha);
}
