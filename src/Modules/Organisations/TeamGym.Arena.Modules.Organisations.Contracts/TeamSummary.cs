using TeamGym.Arena.Common;

namespace TeamGym.Arena.Modules.Organisations.Contracts;

/// <summary>
/// A team as other modules see it. It holds no gymnast personal data, only counts of the gymnasts in the team, which
/// is enough to check a section's gender composition, such as the <b>Mixed</b> ratio (ADR-0006).
/// </summary>
/// <param name="GymnastCount">The number of gymnasts in the team.</param>
/// <param name="FemaleGymnastCount">The number of gymnasts in the team who compete as female.</param>
/// <param name="MaleGymnastCount">The number of gymnasts in the team who compete as male.</param>
public sealed record TeamSummary(
    TeamId Id,
    OrganisationId OrganisationId,
    string Name,
    Section Section,
    int GymnastCount,
    int FemaleGymnastCount,
    int MaleGymnastCount);
