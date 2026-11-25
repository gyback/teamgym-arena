using TeamGym.Arena.Common.Identifiers;

namespace TeamGym.Arena.Modules.Organisations.Domain.Organisations;

internal readonly record struct OrganisationMembershipId(Guid Value) : IStronglyTypedId<OrganisationMembershipId>
{
    public static OrganisationMembershipId From(Guid value) => new(value);
}
