using TeamGym.Arena.Common;
using TeamGym.Arena.Common.Results;

namespace TeamGym.Arena.Modules.Competitions.Domain.RuleSets;

/// <summary>
/// The sections, disciplines and classes that a rule set or a competition offers, and the team rules of each
/// (ADR-0006). It is immutable, so a competition that holds it holds a snapshot that later changes to a preset
/// cannot alter.
/// </summary>
internal sealed class TeamGymRules
{
    private TeamGymRules(
        IReadOnlyList<SectionRule> sections,
        IReadOnlyList<Discipline> disciplines,
        IReadOnlyList<ClassRules> classes)
    {
        Sections = sections;
        Disciplines = disciplines;
        Classes = classes;
    }

    public IReadOnlyList<SectionRule> Sections { get; }

    public IReadOnlyList<Discipline> Disciplines { get; }

    public IReadOnlyList<ClassRules> Classes { get; }

    /// <summary>
    /// Creates the rules. Each list needs at least one entry, and no section, discipline or class code may appear
    /// twice.
    /// </summary>
    public static Result<TeamGymRules> Create(
        IEnumerable<SectionRule> sections,
        IEnumerable<Discipline> disciplines,
        IEnumerable<ClassRules> classes)
    {
        ArgumentNullException.ThrowIfNull(sections);
        ArgumentNullException.ThrowIfNull(disciplines);
        ArgumentNullException.ThrowIfNull(classes);

        var sectionList = sections.ToList().AsReadOnly();
        var disciplineList = disciplines.ToList().AsReadOnly();
        var classList = classes.ToList().AsReadOnly();

        if (sectionList.Count == 0
            || sectionList.Any(s => s is null || !Enum.IsDefined(s.Section) || !Enum.IsDefined(s.GenderComposition))
            || sectionList.DistinctBy(s => s.Section).Count() != sectionList.Count)
        {
            return Result.Failure<TeamGymRules>(RuleSetErrors.InvalidSections);
        }

        if (disciplineList.Count == 0
            || !disciplineList.All(Enum.IsDefined)
            || disciplineList.Distinct().Count() != disciplineList.Count)
        {
            return Result.Failure<TeamGymRules>(RuleSetErrors.InvalidDisciplines);
        }

        if (classList.Count == 0
            || classList.Any(c => c is null)
            || classList.DistinctBy(c => c.Code, StringComparer.Ordinal).Count() != classList.Count)
        {
            return Result.Failure<TeamGymRules>(RuleSetErrors.InvalidClasses);
        }

        return Result.Success(new TeamGymRules(sectionList, disciplineList, classList));
    }

    public SectionRule? FindSection(Section section) => Sections.FirstOrDefault(s => s.Section == section);

    /// <summary>Finds a class by its code, ignoring case and surrounding white space.</summary>
    public ClassRules? FindClass(string? classCode)
    {
        var code = ClassRules.NormaliseCode(classCode);

        return Classes.FirstOrDefault(c => c.Code == code);
    }

    /// <summary>
    /// Narrows the rules to the given sections, classes and disciplines, as an organiser does for one competition.
    /// Each one must already be offered here. The result keeps the order of these rules.
    /// </summary>
    public Result<TeamGymRules> Restrict(
        IReadOnlyCollection<Section> sections,
        IReadOnlyCollection<string> classCodes,
        IReadOnlyCollection<Discipline> disciplines)
    {
        ArgumentNullException.ThrowIfNull(sections);
        ArgumentNullException.ThrowIfNull(classCodes);
        ArgumentNullException.ThrowIfNull(disciplines);

        if (sections.Any(s => FindSection(s) is null))
        {
            return Result.Failure<TeamGymRules>(RuleSetErrors.SectionNotInRuleSet);
        }

        if (disciplines.Any(d => !Disciplines.Contains(d)))
        {
            return Result.Failure<TeamGymRules>(RuleSetErrors.DisciplineNotInRuleSet);
        }

        var codes = classCodes.Select(ClassRules.NormaliseCode).ToHashSet(StringComparer.Ordinal);
        if (codes.Any(code => FindClass(code) is null))
        {
            return Result.Failure<TeamGymRules>(RuleSetErrors.ClassNotInRuleSet);
        }

        return Create(
            Sections.Where(s => sections.Contains(s.Section)),
            Disciplines.Where(disciplines.Contains),
            Classes.Where(c => codes.Contains(c.Code)));
    }

    /// <summary>Replaces the team size rules of one class, as an organiser may for one competition.</summary>
    public Result<TeamGymRules> WithClassComposition(string? classCode, TeamComposition composition)
    {
        ArgumentNullException.ThrowIfNull(composition);

        var target = FindClass(classCode);
        if (target is null)
        {
            return Result.Failure<TeamGymRules>(RuleSetErrors.ClassNotInRuleSet);
        }

        var classes = Classes.Select(c => c == target ? c.WithComposition(composition) : c).ToList().AsReadOnly();

        return Result.Success(new TeamGymRules(Sections, Disciplines, classes));
    }

    /// <summary>
    /// Checks that a team with <paramref name="roster"/> may compete in <paramref name="section"/> and the class
    /// <paramref name="classCode"/> under these rules. The section and class must be offered, the squad size must be
    /// within the bounds of the class, and the genders must match the composition of the section.
    /// </summary>
    public Result ValidateTeam(Section section, string? classCode, TeamRoster roster)
    {
        var sectionRule = FindSection(section);
        if (sectionRule is null)
        {
            return Result.Failure(TeamCompositionErrors.SectionNotOffered);
        }

        var classRules = FindClass(classCode);
        if (classRules is null)
        {
            return Result.Failure(TeamCompositionErrors.ClassNotOffered);
        }

        if (roster.GymnastCount < classRules.Composition.MinSquadSize)
        {
            return Result.Failure(TeamCompositionErrors.SquadTooSmall);
        }

        if (roster.GymnastCount > classRules.Composition.MaxSquadSize)
        {
            return Result.Failure(TeamCompositionErrors.SquadTooLarge);
        }

        return sectionRule.GenderComposition switch
        {
            GenderComposition.FemaleOnly when roster.MaleGymnastCount > 0 =>
                Result.Failure(TeamCompositionErrors.FemaleGymnastsOnly),
            GenderComposition.MaleOnly when roster.FemaleGymnastCount > 0 =>
                Result.Failure(TeamCompositionErrors.MaleGymnastsOnly),
            GenderComposition.EqualFemaleAndMale when roster.FemaleGymnastCount != roster.MaleGymnastCount =>
                Result.Failure(TeamCompositionErrors.EqualFemaleAndMaleGymnasts),
            _ => Result.Success(),
        };
    }
}
