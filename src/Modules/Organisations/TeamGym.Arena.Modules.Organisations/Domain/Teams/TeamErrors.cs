using TeamGym.Arena.Common.Results;

namespace TeamGym.Arena.Modules.Organisations.Domain.Teams;

internal static class TeamErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Organisations.TeamNotFound", "The organisation has no such team.");

    public static readonly Error NameRequired = Error.Validation(
        "Organisations.TeamNameRequired", "A team needs a name.");

    public static readonly Error NameTooLong = Error.Validation(
        "Organisations.TeamNameTooLong", $"A team name is at most {Team.NameMaxLength} characters.");

    public static readonly Error GymnastAlreadyInTeam = Error.Conflict(
        "Organisations.GymnastAlreadyInTeam", "The gymnast is already in the team.");

    public static readonly Error GymnastNotInTeam = Error.NotFound(
        "Organisations.GymnastNotInTeam", "The gymnast is not in the team.");
}
