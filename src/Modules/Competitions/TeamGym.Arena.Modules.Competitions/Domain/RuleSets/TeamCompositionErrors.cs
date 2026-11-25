using TeamGym.Arena.Common.Results;

namespace TeamGym.Arena.Modules.Competitions.Domain.RuleSets;

/// <summary>Why a team does not meet a competition's rules. Like every error, they hold no personal data.</summary>
internal static class TeamCompositionErrors
{
    public static readonly Error SectionNotOffered = Error.Validation(
        "Competitions.SectionNotOffered", "The competition does not offer the team's section.");

    public static readonly Error ClassNotOffered = Error.Validation(
        "Competitions.ClassNotOffered", "The competition does not offer the class.");

    public static readonly Error SquadTooSmall = Error.Validation(
        "Competitions.SquadTooSmall", "The team has fewer gymnasts than the class allows.");

    public static readonly Error SquadTooLarge = Error.Validation(
        "Competitions.SquadTooLarge", "The team has more gymnasts than the class allows.");

    public static readonly Error FemaleGymnastsOnly = Error.Validation(
        "Competitions.FemaleGymnastsOnly", "Every gymnast in a team in this section must compete as female.");

    public static readonly Error MaleGymnastsOnly = Error.Validation(
        "Competitions.MaleGymnastsOnly", "Every gymnast in a team in this section must compete as male.");

    public static readonly Error EqualFemaleAndMaleGymnasts = Error.Validation(
        "Competitions.EqualFemaleAndMaleGymnasts",
        "A team in this section needs as many gymnasts competing as female as competing as male.");
}
