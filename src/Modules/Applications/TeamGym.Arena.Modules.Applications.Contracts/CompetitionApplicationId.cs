using TeamGym.Arena.Common.Identifiers;

namespace TeamGym.Arena.Modules.Applications.Contracts;

/// <summary>Identifies a club's application to a competition (ADR-0006).</summary>
public readonly record struct CompetitionApplicationId(Guid Value) : IStronglyTypedId<CompetitionApplicationId>
{
    public static CompetitionApplicationId From(Guid value) => new(value);
}
