using TeamGym.Arena.Common;
using TeamGym.Arena.Modules.Organisations.Domain.Gymnasts;
using TeamGym.Arena.Modules.Organisations.Domain.Organisations;
using TeamGym.Arena.Modules.Organisations.Domain.Teams;
using static TeamGym.Arena.Modules.Organisations.UnitTests.Domain.DomainFixture;

namespace TeamGym.Arena.Modules.Organisations.UnitTests.Domain;

public sealed class TeamTests
{
    private readonly TimeProvider _clock = Clock();

    [Fact]
    public void CreateMakesATeamOwnedByTheClub()
    {
        var club = Club(_clock);

        var result = Team.Create(club, "  Senior Mixed  ", Section.Mixed, _clock);

        var team = result.Value;
        Assert.Equal(club.Id, team.OrganisationId);
        Assert.True(team.IsOwnedBy(club.Id));
        Assert.Equal("Senior Mixed", team.Name);
        Assert.Equal(Section.Mixed, team.Section);
        Assert.Empty(team.GymnastIds);
    }

    [Fact]
    public void ATeamIsNotOwnedByAnotherClub()
    {
        var team = NewTeam(Club(_clock, name: "Club A"), _clock);

        Assert.False(team.IsOwnedBy(Club(_clock, name: "Club B").Id));
    }

    [Fact]
    public void OnlyAClubOwnsTeams()
    {
        var result = Team.Create(Region(_clock), "Region Team", Section.Women, _clock);

        Assert.Equal(OrganisationErrors.NotAClub, result.Error);
    }

    [Fact]
    public void AnArchivedClubCannotCreateTeams()
    {
        var club = Club(_clock);
        club.Archive();

        var result = Team.Create(club, "Senior Mixed", Section.Mixed, _clock);

        Assert.Equal(OrganisationErrors.Archived, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public void CreateRequiresAName(string? name)
    {
        var result = Team.Create(Club(_clock), name, Section.Men, _clock);

        Assert.Equal(TeamErrors.NameRequired, result.Error);
    }

    [Fact]
    public void RenameRejectsATooLongName()
    {
        var team = NewTeam(Club(_clock), _clock);

        var result = team.Rename(new string('a', Team.NameMaxLength + 1));

        Assert.Equal(TeamErrors.NameTooLong, result.Error);
        Assert.Equal("Senior Team", team.Name);
    }

    [Fact]
    public void AddGymnastAddsAGymnastOfTheSameClub()
    {
        var club = Club(_clock);
        var team = NewTeam(club, _clock);
        var gymnast = NewGymnast(club, _clock);

        var result = team.AddGymnast(gymnast);

        Assert.True(result.IsSuccess);
        Assert.Equal(gymnast.Id, Assert.Single(team.GymnastIds));
    }

    [Fact]
    public void AddGymnastRejectsAGymnastOfAnotherClub()
    {
        var team = NewTeam(Club(_clock, name: "Club A"), _clock);
        var gymnastOfB = NewGymnast(Club(_clock, name: "Club B"), _clock);

        var result = team.AddGymnast(gymnastOfB);

        Assert.Equal(GymnastErrors.NotFound, result.Error);
        Assert.Empty(team.GymnastIds);
    }

    [Fact]
    public void AddGymnastRejectsAGymnastAlreadyInTheTeam()
    {
        var club = Club(_clock);
        var team = NewTeam(club, _clock);
        var gymnast = NewGymnast(club, _clock);
        team.AddGymnast(gymnast);

        var result = team.AddGymnast(gymnast);

        Assert.Equal(TeamErrors.GymnastAlreadyInTeam, result.Error);
        Assert.Single(team.GymnastIds);
    }

    [Fact]
    public void AGymnastCanBeInSeveralTeamsOfTheirClub()
    {
        var club = Club(_clock);
        var gymnast = NewGymnast(club, _clock);
        var mixed = NewTeam(club, _clock);
        var women = Team.Create(club, "Senior Women", Section.Women, _clock).Value;

        Assert.True(mixed.AddGymnast(gymnast).IsSuccess);
        Assert.True(women.AddGymnast(gymnast).IsSuccess);
    }

    [Fact]
    public void RemoveGymnastRemovesTheGymnast()
    {
        var club = Club(_clock);
        var team = NewTeam(club, _clock);
        var gymnast = NewGymnast(club, _clock);
        team.AddGymnast(gymnast);

        var result = team.RemoveGymnast(gymnast.Id);

        Assert.True(result.IsSuccess);
        Assert.Empty(team.GymnastIds);
    }

    [Fact]
    public void RemoveGymnastReportsAGymnastNotInTheTeam()
    {
        var club = Club(_clock);
        var team = NewTeam(club, _clock);

        var result = team.RemoveGymnast(NewGymnast(club, _clock).Id);

        Assert.Equal(TeamErrors.GymnastNotInTeam, result.Error);
    }
}
