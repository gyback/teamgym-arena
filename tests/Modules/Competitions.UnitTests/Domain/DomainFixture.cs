using Microsoft.Extensions.Time.Testing;
using TeamGym.Arena.Common;
using TeamGym.Arena.Modules.Competitions.Contracts;
using TeamGym.Arena.Modules.Competitions.Domain.Competitions;
using TeamGym.Arena.Modules.Competitions.Domain.RuleSets;
using TeamGym.Arena.Modules.Organisations.Contracts;

namespace TeamGym.Arena.Modules.Competitions.UnitTests.Domain;

/// <summary>Builds valid domain objects with synthetic data, so each test states only what it is about.</summary>
internal static class DomainFixture
{
    public static readonly DateTimeOffset Now = new(2027, 3, 14, 9, 30, 0, TimeSpan.Zero);

    public static readonly OrganisationId Organiser = OrganisationId.From(Guid.CreateVersion7(Now));

    public static FakeTimeProvider Clock() => new(Now);

    public static OrganisationId AnotherOrganisation() => OrganisationId.From(Guid.CreateVersion7(Now));

    public static TeamComposition Composition(
        int minSquadSize = 8,
        int maxSquadSize = 10,
        int minFloorGymnasts = 8,
        int maxFloorGymnasts = 10,
        int gymnastsPerRound = 6) =>
        TeamComposition.Create(minSquadSize, maxSquadSize, minFloorGymnasts, maxFloorGymnasts, gymnastsPerRound).Value;

    public static ClassRules Class(string code, TeamComposition? composition = null) =>
        ClassRules.Create(code, code, composition ?? Composition()).Value;

    /// <summary>A small custom rule set that differs from both presets.</summary>
    public static RuleSet CustomRuleSet() => RuleSet.Create(
        RuleSetId.From(Guid.CreateVersion7(Now)),
        "TEST",
        "Test Rules",
        version: 3,
        TeamGymRules.Create(
            [new SectionRule(Section.Open, GenderComposition.Any)],
            [Discipline.Floor],
            [Class("OPEN-CLASS", Composition(4, 6, 4, 6, 4))]).Value).Value;

    public static Competition DraftCompetition(TimeProvider clock, RuleSet? ruleSet = null) =>
        Competition.Create(Organiser, "Spring Cup", ruleSet ?? RuleSetPresets.SwedishGymnasticsFederation, clock).Value;

    public static Competition PublishedCompetition(TimeProvider clock, RuleSet? ruleSet = null)
    {
        var competition = DraftCompetition(clock, ruleSet);
        competition.Publish();

        return competition;
    }
}
