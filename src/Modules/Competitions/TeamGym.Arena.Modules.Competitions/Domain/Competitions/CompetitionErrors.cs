using TeamGym.Arena.Common.Results;

namespace TeamGym.Arena.Modules.Competitions.Domain.Competitions;

internal static class CompetitionErrors
{
    public static readonly Error NameRequired = Error.Validation(
        "Competitions.NameRequired", "A competition needs a name.");

    public static readonly Error NameTooLong = Error.Validation(
        "Competitions.NameTooLong", $"A competition name is at most {Competition.NameMaxLength} characters.");

    public static readonly Error RulesFixed = Error.Conflict(
        "Competitions.RulesFixed", "The rules of a published competition cannot change.");

    public static readonly Error AlreadyPublished = Error.Conflict(
        "Competitions.AlreadyPublished", "The competition is already published.");
}
