using TeamGym.Arena.Common;
using TeamGym.Arena.Modules.Competitions.Domain.Competitions;
using TeamGym.Arena.Modules.Competitions.Domain.RuleSets;
using static TeamGym.Arena.Modules.Competitions.UnitTests.Domain.DomainFixture;

namespace TeamGym.Arena.Modules.Competitions.UnitTests.Domain;

public sealed class CompetitionTests
{
    private readonly TimeProvider _clock = Clock();

    [Fact]
    public void CreateMakesADraftThatCopiesTheWholeRuleSet()
    {
        var ruleSet = RuleSetPresets.EuropeanGymnastics;

        var result = Competition.Create(Organiser, "  Spring Cup  ", ruleSet, _clock);

        var competition = result.Value;
        Assert.Equal("Spring Cup", competition.Name);
        Assert.Equal(Organiser, competition.OrganiserOrganisationId);
        Assert.True(competition.IsOrganisedBy(Organiser));
        Assert.Equal(ruleSet.Id, competition.RuleSetId);
        Assert.Equal(ruleSet.Version, competition.RuleSetVersion);
        Assert.Same(ruleSet.Rules, competition.Rules);
        Assert.Equal(CompetitionStatus.Draft, competition.Status);
        Assert.Equal(7, competition.Id.Value.Version);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public void CreateRequiresAName(string? name)
    {
        var result = Competition.Create(Organiser, name, RuleSetPresets.EuropeanGymnastics, _clock);

        Assert.Equal(CompetitionErrors.NameRequired, result.Error);
    }

    [Fact]
    public void RenameRejectsATooLongName()
    {
        var competition = DraftCompetition(_clock);

        var result = competition.Rename(new string('a', Competition.NameMaxLength + 1));

        Assert.Equal(CompetitionErrors.NameTooLong, result.Error);
        Assert.Equal("Spring Cup", competition.Name);
    }

    [Fact]
    public void ADraftIsVisibleOnlyToItsOrganiser()
    {
        var competition = DraftCompetition(_clock);

        Assert.True(competition.IsVisibleTo(Organiser));
        Assert.False(competition.IsVisibleTo(AnotherOrganisation()));
    }

    [Fact]
    public void APublishedCompetitionIsVisibleToEveryOrganisation()
    {
        var competition = PublishedCompetition(_clock);

        Assert.True(competition.IsVisibleTo(Organiser));
        Assert.True(competition.IsVisibleTo(AnotherOrganisation()));
        Assert.False(competition.IsOrganisedBy(AnotherOrganisation()));
    }

    [Fact]
    public void ToSummaryReportsTheOrganiserAndWhetherItIsPublished()
    {
        var competition = DraftCompetition(_clock);

        var draft = competition.ToSummary();
        competition.Publish();
        var published = competition.ToSummary();

        Assert.Equal(competition.Id, draft.Id);
        Assert.Equal(Organiser, draft.OrganiserOrganisationId);
        Assert.False(draft.IsPublished);
        Assert.True(published.IsPublished);
    }

    [Fact]
    public void PublishTwiceFails()
    {
        var competition = PublishedCompetition(_clock);

        var result = competition.Publish();

        Assert.Equal(CompetitionErrors.AlreadyPublished, result.Error);
    }

    [Fact]
    public void SelectRulesNarrowsWhatTheCompetitionOffers()
    {
        var competition = DraftCompetition(_clock);

        var result = competition.SelectRules(
            RuleSetPresets.SwedishGymnasticsFederation,
            [Section.Open],
            ["YOUTH"],
            [Discipline.Floor, Discipline.Trampet]);

        Assert.True(result.IsSuccess);
        Assert.Equal(TeamCompositionErrors.SectionNotOffered, competition.ValidateTeam(Section.Women, "YOUTH", new TeamRoster(8, 0)).Error);
        Assert.Equal(TeamCompositionErrors.ClassNotOffered, competition.ValidateTeam(Section.Open, "SENIOR", new TeamRoster(4, 4)).Error);
        Assert.True(competition.ValidateTeam(Section.Open, "YOUTH", new TeamRoster(4, 4)).IsSuccess);
        Assert.Equal([Discipline.Floor, Discipline.Trampet], competition.Rules.Disciplines);
    }

    [Fact]
    public void SelectRulesCanSwitchToAnotherRuleSet()
    {
        var competition = DraftCompetition(_clock, RuleSetPresets.SwedishGymnasticsFederation);
        var custom = CustomRuleSet();

        competition.SelectRules(custom, [Section.Open], ["OPEN-CLASS"], [Discipline.Floor]);

        Assert.Equal(custom.Id, competition.RuleSetId);
        Assert.Equal(3, competition.RuleSetVersion);
        Assert.True(competition.ValidateTeam(Section.Open, "OPEN-CLASS", new TeamRoster(2, 2)).IsSuccess);
    }

    [Fact]
    public void SelectRulesKeepsTheCurrentRulesWhenTheSelectionIsInvalid()
    {
        var competition = DraftCompetition(_clock, RuleSetPresets.EuropeanGymnastics);
        var rules = competition.Rules;

        var result = competition.SelectRules(
            RuleSetPresets.EuropeanGymnastics, [Section.Open], ["SENIOR"], [Discipline.Floor]);

        Assert.Equal(RuleSetErrors.SectionNotInRuleSet, result.Error);
        Assert.Same(rules, competition.Rules);
    }

    [Fact]
    public void TailorClassCompositionChangesTheTeamSizeForThisCompetitionOnly()
    {
        var competition = DraftCompetition(_clock, RuleSetPresets.EuropeanGymnastics);
        var roster = new TeamRoster(6, 0);

        var result = competition.TailorClassComposition("SENIOR", Composition(6, 12, 6, 12, 6));

        Assert.True(result.IsSuccess);
        Assert.True(competition.ValidateTeam(Section.Women, "SENIOR", roster).IsSuccess);
        Assert.Equal(
            TeamCompositionErrors.SquadTooSmall,
            RuleSetPresets.EuropeanGymnastics.Rules.ValidateTeam(Section.Women, "SENIOR", roster).Error);
    }

    [Fact]
    public void TailorClassCompositionRejectsAClassTheCompetitionDoesNotOffer()
    {
        var competition = DraftCompetition(_clock, RuleSetPresets.EuropeanGymnastics);

        var result = competition.TailorClassComposition("YOUTH", Composition());

        Assert.Equal(RuleSetErrors.ClassNotInRuleSet, result.Error);
    }

    [Fact]
    public void ThePublishedRulesAreFixed()
    {
        var competition = PublishedCompetition(_clock, RuleSetPresets.EuropeanGymnastics);
        var rules = competition.Rules;

        var selected = competition.SelectRules(
            RuleSetPresets.EuropeanGymnastics, [Section.Women], ["SENIOR"], [Discipline.Floor]);
        var tailored = competition.TailorClassComposition("SENIOR", Composition(6, 12, 6, 12, 6));

        Assert.Equal(CompetitionErrors.RulesFixed, selected.Error);
        Assert.Equal(CompetitionErrors.RulesFixed, tailored.Error);
        Assert.Same(rules, competition.Rules);
    }
}
