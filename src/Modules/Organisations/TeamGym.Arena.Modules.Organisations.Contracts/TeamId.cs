using TeamGym.Arena.Common.Identifiers;

namespace TeamGym.Arena.Modules.Organisations.Contracts;

/// <summary>Identifies a team, which a club owns.</summary>
public readonly record struct TeamId(Guid Value) : IStronglyTypedId<TeamId>
{
    public static TeamId From(Guid value) => new(value);
}
