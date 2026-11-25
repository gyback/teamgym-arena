using TeamGym.Arena.Common;
using TeamGym.Arena.Modules.Competitions.Contracts;
using TeamGym.Arena.Modules.Competitions.Domain.RuleSets;

namespace TeamGym.Arena.Modules.Competitions.UnitTests.Domain;

public sealed class RuleSetPresetsTests
{
    [Fact]
    public void EveryPresetHasADistinctVersion7IdAndCode()
    {
        Assert.All(RuleSetPresets.All, preset => Assert.Equal(7, preset.Id.Value.Version));
        Assert.Equal(RuleSetPresets.All.Count, RuleSetPresets.All.Select(p => p.Id).Distinct().Count());
        Assert.Equal(RuleSetPresets.All.Count, RuleSetPresets.All.Select(p => p.Code).Distinct().Count());
    }

    [Fact]
    public void FindReturnsThePresetWithTheId()
    {
        var preset = RuleSetPresets.SwedishGymnasticsFederation;

        Assert.Same(preset, RuleSetPresets.Find(preset.Id));
        Assert.Null(RuleSetPresets.Find(RuleSetId.From(Guid.Empty)));
    }

    [Fact]
    public void EuropeanGymnasticsOffersWomenMenAndMixedWithAFiftyFiftyMixedSplit()
    {
        var rules = RuleSetPresets.EuropeanGymnastics.Rules;

        Assert.Equal("EG-TEAMGYM", RuleSetPresets.EuropeanGymnastics.Code);
        Assert.Equal(
            [
                new SectionRule(Section.Women, GenderComposition.FemaleOnly),
                new SectionRule(Section.Men, GenderComposition.MaleOnly),
                new SectionRule(Section.Mixed, GenderComposition.EqualFemaleAndMale),
            ],
            rules.Sections);
        Assert.Null(rules.FindSection(Section.Open));
    }

    [Fact]
    public void EuropeanGymnasticsOffersJuniorAndSeniorWithEightToTenGymnastsAndSixPerRound()
    {
        var rules = RuleSetPresets.EuropeanGymnastics.Rules;

        Assert.Equal([Discipline.Floor, Discipline.Tumble, Discipline.Trampet], rules.Disciplines);
        Assert.Equal(["JUNIOR", "SENIOR"], rules.Classes.Select(c => c.Code));
        Assert.All(rules.Classes, c =>
        {
            Assert.Equal((8, 10), (c.Composition.MinSquadSize, c.Composition.MaxSquadSize));
            Assert.Equal((8, 10), (c.Composition.MinFloorGymnasts, c.Composition.MaxFloorGymnasts));
            Assert.Equal(6, c.Composition.GymnastsPerRound);
        });
    }

    [Fact]
    public void SwedishGymnasticsFederationAddsTheOpenSectionWithAnyComposition()
    {
        var rules = RuleSetPresets.SwedishGymnasticsFederation.Rules;

        Assert.Equal("SWE-TRUPP", RuleSetPresets.SwedishGymnasticsFederation.Code);
        Assert.Equal(
            [Section.Women, Section.Men, Section.Mixed, Section.Open],
            rules.Sections.Select(s => s.Section));
        Assert.Equal(GenderComposition.EqualFemaleAndMale, rules.FindSection(Section.Mixed)!.GenderComposition);
        Assert.Equal(GenderComposition.Any, rules.FindSection(Section.Open)!.GenderComposition);
    }

    [Fact]
    public void SwedishGymnasticsFederationOffersNationalClassesWithLargerYouthSquads()
    {
        var rules = RuleSetPresets.SwedishGymnasticsFederation.Rules;

        Assert.Equal(
            ["SENIOR", "JUNIOR", "YOUTH", "LEVEL-9", "LEVEL-8", "LEVEL-7", "LEVEL-6", "LEVEL-5", "LEVEL-4", "LEVEL-3", "LEVEL-2", "LEVEL-1"],
            rules.Classes.Select(c => c.Code));
        Assert.Equal(
            (8, 10),
            (rules.FindClass("SENIOR")!.Composition.MinSquadSize, rules.FindClass("SENIOR")!.Composition.MaxSquadSize));
        Assert.Equal(
            (6, 16),
            (rules.FindClass("YOUTH")!.Composition.MinSquadSize, rules.FindClass("YOUTH")!.Composition.MaxSquadSize));
        Assert.All(rules.Classes, c => Assert.Equal(6, c.Composition.GymnastsPerRound));
    }
}
