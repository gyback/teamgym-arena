using TeamGym.Arena.Modules.Organisations.Contracts;
using TeamGym.Arena.Modules.Users.Contracts;

namespace TeamGym.Arena.Modules.Organisations.Domain.Organisations;

/// <summary>
/// Gives a user a role in one organisation (ADR-0006). A user can hold memberships in many organisations, so a
/// coach in two clubs needs only one account. It is part of the <see cref="Organisation"/> aggregate, which keeps
/// its invariants.
/// </summary>
internal sealed class OrganisationMembership
{
    private readonly HashSet<TeamId> _teamIds = [];

    internal OrganisationMembership(
        OrganisationMembershipId id,
        OrganisationId organisationId,
        UserId userId,
        OrganisationRole role,
        DateTimeOffset joinedAt)
    {
        Id = id;
        OrganisationId = organisationId;
        UserId = userId;
        Role = role;
        JoinedAt = joinedAt;
    }

    public OrganisationMembershipId Id { get; }

    public OrganisationId OrganisationId { get; }

    public UserId UserId { get; }

    public OrganisationRole Role { get; }

    /// <summary>When the user joined, in UTC.</summary>
    public DateTimeOffset JoinedAt { get; }

    /// <summary>
    /// The teams a coach is assigned to. Only a <see cref="OrganisationRole.Coach"/> membership has team
    /// assignments; what an assignment allows is decided by the permission model (ADR-0008).
    /// </summary>
    public IReadOnlyCollection<TeamId> TeamIds => _teamIds;

    internal bool AssignTeam(TeamId teamId) => _teamIds.Add(teamId);

    internal bool UnassignTeam(TeamId teamId) => _teamIds.Remove(teamId);
}
