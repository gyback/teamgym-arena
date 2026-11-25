using TeamGym.Arena.Modules.Organisations.Contracts;

namespace TeamGym.Arena.Modules.Organisations.Domain.Teams;

/// <summary>A gymnast's place in a team. It is part of the <see cref="Team"/> aggregate, which keeps its invariants.</summary>
internal sealed class TeamMember(TeamId teamId, GymnastId gymnastId)
{
    public TeamId TeamId { get; } = teamId;

    public GymnastId GymnastId { get; } = gymnastId;
}
