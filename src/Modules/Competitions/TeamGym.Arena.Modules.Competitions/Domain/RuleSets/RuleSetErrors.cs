using TeamGym.Arena.Common.Results;

namespace TeamGym.Arena.Modules.Competitions.Domain.RuleSets;

/// <summary>Errors in the definition of a rule set, or in narrowing one for a competition.</summary>
internal static class RuleSetErrors
{
    public static readonly Error InvalidCode = Error.Validation(
        "Competitions.RuleSetInvalidCode", $"A rule set needs a code of at most {RuleSet.CodeMaxLength} characters.");

    public static readonly Error InvalidName = Error.Validation(
        "Competitions.RuleSetInvalidName", $"A rule set needs a name of at most {RuleSet.NameMaxLength} characters.");

    public static readonly Error InvalidVersion = Error.Validation(
        "Competitions.RuleSetInvalidVersion", "A rule set version starts at 1.");

    public static readonly Error InvalidSections = Error.Validation(
        "Competitions.RuleSetInvalidSections", "The rules need at least one section, and each section only once.");

    public static readonly Error InvalidDisciplines = Error.Validation(
        "Competitions.RuleSetInvalidDisciplines",
        "The rules need at least one discipline, and each discipline only once.");

    public static readonly Error InvalidClasses = Error.Validation(
        "Competitions.RuleSetInvalidClasses", "The rules need at least one class, and each class code only once.");

    public static readonly Error InvalidClassCode = Error.Validation(
        "Competitions.RuleSetInvalidClassCode",
        $"A class code is 1 to {ClassRules.CodeMaxLength} ASCII letters, digits or hyphens.");

    public static readonly Error InvalidClassName = Error.Validation(
        "Competitions.RuleSetInvalidClassName",
        $"A class needs a name of at most {ClassRules.NameMaxLength} characters.");

    public static readonly Error InvalidSquadSize = Error.Validation(
        "Competitions.RuleSetInvalidSquadSize", "The minimum squad size must be at least 1 and at most the maximum.");

    public static readonly Error InvalidFloorGymnasts = Error.Validation(
        "Competitions.RuleSetInvalidFloorGymnasts",
        "The Floor gymnast bounds must be at least 1, in order, and within the squad size bounds.");

    public static readonly Error InvalidGymnastsPerRound = Error.Validation(
        "Competitions.RuleSetInvalidGymnastsPerRound",
        "The gymnasts per Tumble and Trampet round must be at least 1 and at most the minimum squad size.");

    public static readonly Error SectionNotInRuleSet = Error.Validation(
        "Competitions.SectionNotInRuleSet", "The rule set does not offer the section.");

    public static readonly Error DisciplineNotInRuleSet = Error.Validation(
        "Competitions.DisciplineNotInRuleSet", "The rule set does not offer the discipline.");

    public static readonly Error ClassNotInRuleSet = Error.Validation(
        "Competitions.ClassNotInRuleSet", "The rule set does not offer the class.");
}
