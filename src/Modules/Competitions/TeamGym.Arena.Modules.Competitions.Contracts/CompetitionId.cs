using TeamGym.Arena.Common.Identifiers;

namespace TeamGym.Arena.Modules.Competitions.Contracts;

/// <summary>Identifies a competition, which one organisation hosts (ADR-0006).</summary>
public readonly record struct CompetitionId(Guid Value) : IStronglyTypedId<CompetitionId>
{
    public static CompetitionId From(Guid value) => new(value);
}
