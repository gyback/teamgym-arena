using TeamGym.Arena.Common;
using TeamGym.Arena.Modules.Competitions.Contracts;

namespace TeamGym.Arena.Modules.Competitions.Domain.RuleSets;

/// <summary>
/// The built-in rule set presets that ship with the platform (ADR-0006). Their IDs are fixed, so an organisation's
/// default rule set and a competition's source rule set keep pointing at the same preset across deployments. To
/// change a preset's rules, raise its <see cref="RuleSet.Version"/>; competitions that already copied the rules keep
/// their copy.
/// </summary>
internal static class RuleSetPresets
{
    /// <summary>
    /// European Gymnastics TeamGym: sections <b>Women</b>, <b>Men</b> and <b>Mixed</b> (50 % female and 50 % male),
    /// all three disciplines, and the Junior and Senior classes with 8 to 10 gymnasts, all of whom may compete on
    /// <b>Floor</b>, and 6 gymnasts per round on <b>Tumble</b> and <b>Trampet</b>.
    /// </summary>
    public static RuleSet EuropeanGymnastics { get; } = RuleSet.Create(
        RuleSetId.From(new Guid("01968f40-0000-7000-8000-000000000001")),
        "EG-TEAMGYM",
        "European Gymnastics TeamGym",
        version: 1,
        TeamGymRules.Create(
            [
                new SectionRule(Section.Women, GenderComposition.FemaleOnly),
                new SectionRule(Section.Men, GenderComposition.MaleOnly),
                new SectionRule(Section.Mixed, GenderComposition.EqualFemaleAndMale),
            ],
            AllDisciplines,
            [
                Class("JUNIOR", "Junior", Championship),
                Class("SENIOR", "Senior", Championship),
            ]).Value).Value;

    /// <summary>
    /// Swedish Gymnastics Federation TeamGym, the national preset for Swedish and Nordic competitions. It adds the
    /// <b>Open</b> section, which allows any gender composition, and national classes: Senior and Junior with the
    /// European Gymnastics team sizes, and Youth and the national levels 1 to 9 with squads of 6 to 16 gymnasts.
    /// </summary>
    public static RuleSet SwedishGymnasticsFederation { get; } = RuleSet.Create(
        RuleSetId.From(new Guid("01968f40-0000-7000-8000-000000000002")),
        "SWE-TRUPP",
        "Swedish Gymnastics Federation TeamGym",
        version: 1,
        TeamGymRules.Create(
            [
                new SectionRule(Section.Women, GenderComposition.FemaleOnly),
                new SectionRule(Section.Men, GenderComposition.MaleOnly),
                new SectionRule(Section.Mixed, GenderComposition.EqualFemaleAndMale),
                new SectionRule(Section.Open, GenderComposition.Any),
            ],
            AllDisciplines,
            [
                Class("SENIOR", "Senior", Championship),
                Class("JUNIOR", "Junior", Championship),
                Class("YOUTH", "Youth", National),
                .. Enumerable.Range(1, 9).Reverse().Select(level => Class($"LEVEL-{level}", $"Level {level}", National)),
            ]).Value).Value;

    /// <summary>Every built-in preset.</summary>
    public static IReadOnlyList<RuleSet> All { get; } = [EuropeanGymnastics, SwedishGymnasticsFederation];

    private static Discipline[] AllDisciplines => [Discipline.Floor, Discipline.Tumble, Discipline.Trampet];

    /// <summary>8 to 10 gymnasts, all of whom may compete on <b>Floor</b>, and 6 per round.</summary>
    private static TeamComposition Championship => TeamComposition.Create(8, 10, 8, 10, 6).Value;

    /// <summary>6 to 16 gymnasts, all of whom may compete on <b>Floor</b>, and 6 per round.</summary>
    private static TeamComposition National => TeamComposition.Create(6, 16, 6, 16, 6).Value;

    /// <summary>Finds a built-in preset by its ID.</summary>
    /// <returns>The preset, or <see langword="null"/> when no preset has the ID.</returns>
    public static RuleSet? Find(RuleSetId id) => All.FirstOrDefault(r => r.Id == id);

    private static ClassRules Class(string code, string name, TeamComposition composition) =>
        ClassRules.Create(code, name, composition).Value;
}
