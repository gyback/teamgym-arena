using TeamGym.Arena.Common;
using TeamGym.Arena.Modules.Competitions.Domain.RuleSets;

namespace TeamGym.Arena.Modules.Competitions.UnitTests.Domain;

/// <summary>
/// Team composition validation against both built-in presets: European Gymnastics TeamGym (<c>EG</c>) and the
/// Swedish Gymnastics Federation national rules (<c>SWE</c>).
/// </summary>
public sealed class TeamCompositionValidationTests
{
    [Theory]
    [InlineData("EG", Section.Women, "SENIOR", 8, 0)]
    [InlineData("EG", Section.Women, "JUNIOR", 10, 0)]
    [InlineData("EG", Section.Men, "SENIOR", 0, 9)]
    [InlineData("EG", Section.Mixed, "SENIOR", 4, 4)]
    [InlineData("EG", Section.Mixed, "JUNIOR", 5, 5)]
    [InlineData("SWE", Section.Women, "SENIOR", 10, 0)]
    [InlineData("SWE", Section.Mixed, "LEVEL-5", 3, 3)]
    [InlineData("SWE", Section.Open, "YOUTH", 5, 1)]
    [InlineData("SWE", Section.Open, "YOUTH", 0, 16)]
    [InlineData("SWE", Section.Open, "LEVEL-1", 9, 2)]
    public void AValidTeamPasses(string preset, Section section, string classCode, int female, int male)
    {
        var result = Rules(preset).ValidateTeam(section, classCode, new TeamRoster(female, male));

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData("EG", Section.Women, "SENIOR", 7, 0, "Competitions.SquadTooSmall")]
    [InlineData("EG", Section.Women, "SENIOR", 11, 0, "Competitions.SquadTooLarge")]
    [InlineData("EG", Section.Mixed, "JUNIOR", 3, 3, "Competitions.SquadTooSmall")]
    [InlineData("SWE", Section.Women, "SENIOR", 6, 0, "Competitions.SquadTooSmall")]
    [InlineData("SWE", Section.Open, "YOUTH", 5, 0, "Competitions.SquadTooSmall")]
    [InlineData("SWE", Section.Open, "YOUTH", 9, 8, "Competitions.SquadTooLarge")]
    public void ASquadOutsideTheClassBoundsFails(
        string preset, Section section, string classCode, int female, int male, string expectedCode)
    {
        var result = Rules(preset).ValidateTeam(section, classCode, new TeamRoster(female, male));

        Assert.Equal(expectedCode, result.Error?.Code);
    }

    [Theory]
    [InlineData("EG", Section.Women, 9, 1, "Competitions.FemaleGymnastsOnly")]
    [InlineData("SWE", Section.Women, 9, 1, "Competitions.FemaleGymnastsOnly")]
    [InlineData("EG", Section.Men, 1, 9, "Competitions.MaleGymnastsOnly")]
    [InlineData("SWE", Section.Men, 1, 9, "Competitions.MaleGymnastsOnly")]
    [InlineData("EG", Section.Mixed, 5, 4, "Competitions.EqualFemaleAndMaleGymnasts")]
    [InlineData("EG", Section.Mixed, 6, 4, "Competitions.EqualFemaleAndMaleGymnasts")]
    [InlineData("SWE", Section.Mixed, 6, 4, "Competitions.EqualFemaleAndMaleGymnasts")]
    [InlineData("SWE", Section.Mixed, 10, 0, "Competitions.EqualFemaleAndMaleGymnasts")]
    public void AGenderCompositionTheSectionDoesNotAllowFails(
        string preset, Section section, int female, int male, string expectedCode)
    {
        var result = Rules(preset).ValidateTeam(section, "SENIOR", new TeamRoster(female, male));

        Assert.Equal(expectedCode, result.Error?.Code);
    }

    [Fact]
    public void TheOpenSectionIsOfferedOnlyByTheNationalPreset()
    {
        var roster = new TeamRoster(6, 3);

        var european = Rules("EG").ValidateTeam(Section.Open, "SENIOR", roster);
        var national = Rules("SWE").ValidateTeam(Section.Open, "SENIOR", roster);

        Assert.Equal(TeamCompositionErrors.SectionNotOffered, european.Error);
        Assert.True(national.IsSuccess);
    }

    [Fact]
    public void AYouthSquadOfSixIsValidOnlyUnderTheNationalPreset()
    {
        var roster = new TeamRoster(6, 0);

        Assert.Equal(TeamCompositionErrors.SquadTooSmall, Rules("EG").ValidateTeam(Section.Women, "JUNIOR", roster).Error);
        Assert.True(Rules("SWE").ValidateTeam(Section.Women, "YOUTH", roster).IsSuccess);
    }

    [Theory]
    [InlineData("EG", "YOUTH")]
    [InlineData("EG", "LEVEL-9")]
    [InlineData("SWE", "MASTERS")]
    [InlineData("SWE", null)]
    public void AClassTheRulesDoNotOfferFails(string preset, string? classCode)
    {
        var result = Rules(preset).ValidateTeam(Section.Women, classCode, new TeamRoster(8, 0));

        Assert.Equal(TeamCompositionErrors.ClassNotOffered, result.Error);
    }

    [Fact]
    public void TheClassCodeIgnoresCaseAndWhiteSpace()
    {
        var result = Rules("SWE").ValidateTeam(Section.Women, " level-3 ", new TeamRoster(6, 0));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ARosterCannotHaveANegativeCount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TeamRoster(-1, 8));
    }

    private static TeamGymRules Rules(string preset) => preset switch
    {
        "EG" => RuleSetPresets.EuropeanGymnastics.Rules,
        "SWE" => RuleSetPresets.SwedishGymnasticsFederation.Rules,
        _ => throw new ArgumentOutOfRangeException(nameof(preset), preset, null),
    };
}
