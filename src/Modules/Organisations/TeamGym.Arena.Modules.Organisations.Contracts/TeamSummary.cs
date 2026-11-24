using TeamGym.Arena.Common;

namespace TeamGym.Arena.Modules.Organisations.Contracts;

/// <summary>
/// A team as other modules see it. It holds no gymnast personal data, only the number of gymnasts in the team.
/// </summary>
public sealed record TeamSummary(
    TeamId Id,
    OrganisationId OrganisationId,
    string Name,
    Section Section,
    int GymnastCount);
