namespace TeamGym.Arena.Modules.Organisations.Contracts;

/// <summary>
/// Read-only queries that other modules use to verify organisation ownership and team eligibility, for example
/// before <c>Applications</c> accepts a team into a competition (ADR-0004, ADR-0007).
/// </summary>
public interface IOrganisationsContracts
{
    /// <summary>Finds the public summary of an organisation.</summary>
    /// <returns>The summary, or <see langword="null"/> when no such organisation exists.</returns>
    Task<OrganisationSummary?> FindOrganisationAsync(
        OrganisationId organisationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a team that <paramref name="owningOrganisationId"/> owns. The ownership check is part of the query, so a
    /// team that belongs to another organisation is reported as missing.
    /// </summary>
    /// <returns>The team, or <see langword="null"/> when the organisation owns no such team.</returns>
    Task<TeamSummary?> FindOwnedTeamAsync(
        OrganisationId owningOrganisationId,
        TeamId teamId,
        CancellationToken cancellationToken = default);
}
