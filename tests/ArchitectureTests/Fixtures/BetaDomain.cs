namespace TeamGym.Arena.ArchitectureTests.Fixtures.Modules.Beta.Domain;

/// <summary>Allowed: the domain uses only the base class library.</summary>
internal interface IBetaAggregate
{
    DateTimeOffset ScheduledAt { get; }
}
