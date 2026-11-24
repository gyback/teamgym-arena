using TeamGym.Arena.Common.Identifiers;

namespace TeamGym.Arena.Modules.Competitions.Contracts;

/// <summary>Identifies a TeamGym rule set preset (ADR-0006).</summary>
public readonly record struct RuleSetId(Guid Value) : IStronglyTypedId<RuleSetId>
{
    public static RuleSetId From(Guid value) => new(value);
}
