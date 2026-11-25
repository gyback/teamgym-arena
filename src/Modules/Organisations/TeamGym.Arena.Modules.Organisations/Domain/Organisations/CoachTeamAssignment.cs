using TeamGym.Arena.Modules.Organisations.Contracts;

namespace TeamGym.Arena.Modules.Organisations.Domain.Organisations;

/// <summary>
/// Assigns a coach's membership to a team of the same organisation. It is part of the <see cref="Organisation"/>
/// aggregate, which keeps its invariants.
/// </summary>
internal sealed class CoachTeamAssignment(OrganisationMembershipId organisationMembershipId, TeamId teamId)
{
    public OrganisationMembershipId OrganisationMembershipId { get; } = organisationMembershipId;

    public TeamId TeamId { get; } = teamId;
}
