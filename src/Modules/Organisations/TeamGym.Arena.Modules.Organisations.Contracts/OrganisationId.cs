using TeamGym.Arena.Common.Identifiers;

namespace TeamGym.Arena.Modules.Organisations.Contracts;

/// <summary>Identifies an organisation: a club, a region or a federation (ADR-0006).</summary>
public readonly record struct OrganisationId(Guid Value) : IStronglyTypedId<OrganisationId>
{
    public static OrganisationId From(Guid value) => new(value);
}
