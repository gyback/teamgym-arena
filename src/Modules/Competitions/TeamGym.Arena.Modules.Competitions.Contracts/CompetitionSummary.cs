using TeamGym.Arena.Modules.Organisations.Contracts;

namespace TeamGym.Arena.Modules.Competitions.Contracts;

/// <summary>
/// A competition as other modules see it. Other modules use it to apply the cross-organisation visibility rules of
/// ADR-0006, for example to keep an application private between the applying club and the organiser.
/// </summary>
/// <param name="OrganiserOrganisationId">The organisation that hosts the competition.</param>
/// <param name="IsPublished">
/// <see langword="false"/> while the competition is a draft, which only its organiser may see.
/// </param>
public sealed record CompetitionSummary(
    CompetitionId Id,
    OrganisationId OrganiserOrganisationId,
    string Name,
    bool IsPublished);
