using TeamGym.Arena.Common;
using TeamGym.Arena.Modules.Competitions.Domain.RuleSets;
using static TeamGym.Arena.Modules.Competitions.UnitTests.Domain.DomainFixture;

namespace TeamGym.Arena.Modules.Competitions.UnitTests.Domain;

public sealed class TeamGymRulesTests
{
    private static readonly SectionRule[] s_sections =
    [
        new(Section.Women, GenderComposition.FemaleOnly),
        new(Section.Open, GenderComposition.Any),
    ];

    private static readonly Discipline[] s_disciplines = [Discipline.Floor, Discipline.Tumble, Discipline.Trampet];

    [Fact]
    public void CreateRequiresASection()
    {
        var result = TeamGymRules.Create([], s_disciplines, [Class("SENIOR")]);

        Assert.Equal(RuleSetErrors.InvalidSections, result.Error);
    }

    [Fact]
    public void CreateRejectsASectionTwice()
    {
        SectionRule[] sections =
        [
            new(Section.Mixed, GenderComposition.EqualFemaleAndMale),
            new(Section.Mixed, GenderComposition.Any),
        ];

        var result = TeamGymRules.Create(sections, s_disciplines, [Class("SENIOR")]);

        Assert.Equal(RuleSetErrors.InvalidSections, result.Error);
    }

    [Fact]
    public void CreateRejectsAnUndefinedSection()
    {
        var result = TeamGymRules.Create(
            [new SectionRule((Section)42, GenderComposition.Any)], s_disciplines, [Class("SENIOR")]);

        Assert.Equal(RuleSetErrors.InvalidSections, result.Error);
    }

    [Fact]
    public void CreateRequiresADiscipline()
    {
        var result = TeamGymRules.Create(s_sections, [], [Class("SENIOR")]);

        Assert.Equal(RuleSetErrors.InvalidDisciplines, result.Error);
    }

    [Fact]
    public void CreateRejectsADisciplineTwice()
    {
        var result = TeamGymRules.Create(s_sections, [Discipline.Floor, Discipline.Floor], [Class("SENIOR")]);

        Assert.Equal(RuleSetErrors.InvalidDisciplines, result.Error);
    }

    [Fact]
    public void CreateRequiresAClass()
    {
        var result = TeamGymRules.Create(s_sections, s_disciplines, []);

        Assert.Equal(RuleSetErrors.InvalidClasses, result.Error);
    }

    [Fact]
    public void CreateRejectsAClassCodeTwiceIgnoringCase()
    {
        var result = TeamGymRules.Create(s_sections, s_disciplines, [Class("SENIOR"), Class("senior")]);

        Assert.Equal(RuleSetErrors.InvalidClasses, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    [InlineData("SENIOR CLASS")]
    [InlineData("SENIOR_1")]
    [InlineData("ÄLDRE")]
    public void ClassCreateRejectsAnInvalidCode(string? code)
    {
        var result = ClassRules.Create(code, "Senior", Composition());

        Assert.Equal(RuleSetErrors.InvalidClassCode, result.Error);
    }

    [Fact]
    public void ClassCreateNormalisesTheCode()
    {
        var result = ClassRules.Create("  level-9 ", " Level 9 ", Composition());

        Assert.Equal("LEVEL-9", result.Value.Code);
        Assert.Equal("Level 9", result.Value.Name);
    }

    [Fact]
    public void RestrictKeepsOnlyTheSelectedSectionsClassesAndDisciplinesInRuleSetOrder()
    {
        var rules = RuleSetPresets.SwedishGymnasticsFederation.Rules;

        var result = rules.Restrict(
            [Section.Open, Section.Mixed],
            ["youth", "LEVEL-3"],
            [Discipline.Trampet, Discipline.Floor]);

        var restricted = result.Value;
        Assert.Equal([Section.Mixed, Section.Open], restricted.Sections.Select(s => s.Section));
        Assert.Equal(["YOUTH", "LEVEL-3"], restricted.Classes.Select(c => c.Code));
        Assert.Equal([Discipline.Floor, Discipline.Trampet], restricted.Disciplines);
    }

    [Fact]
    public void RestrictLeavesTheOriginalRulesUnchanged()
    {
        var rules = RuleSetPresets.SwedishGymnasticsFederation.Rules;

        rules.Restrict([Section.Women], ["SENIOR"], [Discipline.Floor]);

        Assert.Equal(4, rules.Sections.Count);
        Assert.Equal(3, rules.Disciplines.Count);
    }

    [Fact]
    public void RestrictRejectsASectionTheRuleSetDoesNotOffer()
    {
        var result = RuleSetPresets.EuropeanGymnastics.Rules.Restrict(
            [Section.Open], ["SENIOR"], [Discipline.Floor]);

        Assert.Equal(RuleSetErrors.SectionNotInRuleSet, result.Error);
    }

    [Fact]
    public void RestrictRejectsAClassTheRuleSetDoesNotOffer()
    {
        var result = RuleSetPresets.EuropeanGymnastics.Rules.Restrict(
            [Section.Women], ["YOUTH"], [Discipline.Floor]);

        Assert.Equal(RuleSetErrors.ClassNotInRuleSet, result.Error);
    }

    [Fact]
    public void RestrictRejectsADisciplineTheRulesDoNotOffer()
    {
        var floorOnly = CustomRuleSet().Rules;

        var result = floorOnly.Restrict([Section.Open], ["OPEN-CLASS"], [Discipline.Tumble]);

        Assert.Equal(RuleSetErrors.DisciplineNotInRuleSet, result.Error);
    }

    [Fact]
    public void RestrictRequiresAtLeastOneOfEach()
    {
        var result = RuleSetPresets.EuropeanGymnastics.Rules.Restrict([Section.Women], [], [Discipline.Floor]);

        Assert.Equal(RuleSetErrors.InvalidClasses, result.Error);
    }

    [Fact]
    public void WithClassCompositionReplacesOnlyThatClass()
    {
        var rules = RuleSetPresets.EuropeanGymnastics.Rules;
        var tailored = Composition(6, 12, 6, 12, 6);

        var result = rules.WithClassComposition("junior", tailored);

        Assert.Equal(tailored, result.Value.FindClass("JUNIOR")!.Composition);
        Assert.Equal(Composition(), result.Value.FindClass("SENIOR")!.Composition);
        Assert.Equal(Composition(), rules.FindClass("JUNIOR")!.Composition);
    }

    [Fact]
    public void WithClassCompositionRejectsAnUnknownClass()
    {
        var result = RuleSetPresets.EuropeanGymnastics.Rules.WithClassComposition("YOUTH", Composition());

        Assert.Equal(RuleSetErrors.ClassNotInRuleSet, result.Error);
    }
}
