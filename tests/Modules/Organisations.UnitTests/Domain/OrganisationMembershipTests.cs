using Microsoft.Extensions.Time.Testing;
using TeamGym.Arena.Modules.Organisations.Domain.Organisations;
using TeamGym.Arena.Modules.Organisations.Domain.Teams;
using TeamGym.Arena.Modules.Users.Contracts;
using static TeamGym.Arena.Modules.Organisations.UnitTests.Domain.DomainFixture;

namespace TeamGym.Arena.Modules.Organisations.UnitTests.Domain;

public sealed class OrganisationMembershipTests
{
    private readonly FakeTimeProvider _clock = Clock();
    private readonly UserId _user = UserId.From(Guid.CreateVersion7());

    [Fact]
    public void AddMembershipGivesTheUserARoleInTheOrganisation()
    {
        var club = Club(_clock);

        var result = club.AddMembership(_user, OrganisationRole.Admin, _clock);

        var membership = Assert.Single(club.Memberships);
        Assert.Same(membership, result.Value);
        Assert.Equal(club.Id, membership.OrganisationId);
        Assert.Equal(_user, membership.UserId);
        Assert.Equal(OrganisationRole.Admin, membership.Role);
        Assert.Equal(Now, membership.JoinedAt);
        Assert.Empty(membership.TeamIds);
    }

    [Fact]
    public void AUserCanBeAMemberOfSeveralClubs()
    {
        var clubA = Club(_clock, name: "Club A");
        var clubB = Club(_clock, name: "Club B");

        var inA = clubA.AddMembership(_user, OrganisationRole.Admin, _clock);
        var inB = clubB.AddMembership(_user, OrganisationRole.Coach, _clock);

        Assert.True(inA.IsSuccess);
        Assert.True(inB.IsSuccess);
        Assert.Equal(clubA.Id, inA.Value.OrganisationId);
        Assert.Equal(clubB.Id, inB.Value.OrganisationId);
    }

    [Fact]
    public void AUserCanHoldSeveralRolesInOneOrganisation()
    {
        var club = Club(_clock);

        club.AddMembership(_user, OrganisationRole.Admin, _clock);
        var result = club.AddMembership(_user, OrganisationRole.CompetitionOrganiser, _clock);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, club.Memberships.Count);
    }

    [Fact]
    public void AddMembershipRejectsTheSameRoleTwice()
    {
        var club = Club(_clock);
        club.AddMembership(_user, OrganisationRole.Coach, _clock);

        var result = club.AddMembership(_user, OrganisationRole.Coach, _clock);

        Assert.Equal(OrganisationErrors.DuplicateMembership, result.Error);
        Assert.Single(club.Memberships);
    }

    [Fact]
    public void AnArchivedOrganisationGetsNoNewMembers()
    {
        var club = Club(_clock);
        club.Archive();

        var result = club.AddMembership(_user, OrganisationRole.Admin, _clock);

        Assert.Equal(OrganisationErrors.Archived, result.Error);
        Assert.Empty(club.Memberships);
    }

    [Fact]
    public void AnArchivedOrganisationCannotAssignCoachesToTeams()
    {
        var club = Club(_clock);
        var coach = club.AddMembership(_user, OrganisationRole.Coach, _clock).Value;
        var team = NewTeam(club, _clock);
        club.Archive();

        var result = club.AssignCoachToTeam(coach.Id, team);

        Assert.Equal(OrganisationErrors.Archived, result.Error);
        Assert.Empty(coach.TeamIds);
    }

    [Fact]
    public void RemoveMembershipRemovesOnlyThatMembership()
    {
        var club = Club(_clock);
        var admin = club.AddMembership(_user, OrganisationRole.Admin, _clock).Value;
        var coach = club.AddMembership(_user, OrganisationRole.Coach, _clock).Value;

        var result = club.RemoveMembership(admin.Id);

        Assert.True(result.IsSuccess);
        Assert.Same(coach, Assert.Single(club.Memberships));
    }

    [Fact]
    public void RemoveMembershipReportsAnUnknownMembership()
    {
        var club = Club(_clock);

        var result = club.RemoveMembership(OrganisationMembershipId.From(Guid.CreateVersion7()));

        Assert.Equal(OrganisationErrors.MembershipNotFound, result.Error);
    }

    [Fact]
    public void ACoachCanBeAssignedToTeamsOfTheirClub()
    {
        var club = Club(_clock);
        var coach = club.AddMembership(_user, OrganisationRole.Coach, _clock).Value;
        var junior = NewTeam(club, _clock);
        var senior = NewTeam(club, _clock);

        Assert.True(club.AssignCoachToTeam(coach.Id, junior).IsSuccess);
        Assert.True(club.AssignCoachToTeam(coach.Id, senior).IsSuccess);

        Assert.Equal(2, coach.TeamIds.Count);
        Assert.Contains(junior.Id, coach.TeamIds);
        Assert.Contains(senior.Id, coach.TeamIds);
    }

    [Fact]
    public void ACoachCannotBeAssignedToATeamOfAnotherClub()
    {
        var clubA = Club(_clock, name: "Club A");
        var clubB = Club(_clock, name: "Club B");
        var coachInA = clubA.AddMembership(_user, OrganisationRole.Coach, _clock).Value;
        clubB.AddMembership(_user, OrganisationRole.Coach, _clock);
        var teamOfB = NewTeam(clubB, _clock);

        var result = clubA.AssignCoachToTeam(coachInA.Id, teamOfB);

        Assert.Equal(TeamErrors.NotFound, result.Error);
        Assert.Empty(coachInA.TeamIds);
    }

    [Theory]
    [InlineData(nameof(OrganisationRole.Admin))]
    [InlineData(nameof(OrganisationRole.CompetitionOrganiser))]
    public void OnlyACoachMembershipCanBeAssignedToATeam(string role)
    {
        var club = Club(_clock);
        var membership = club.AddMembership(_user, Enum.Parse<OrganisationRole>(role), _clock).Value;

        var result = club.AssignCoachToTeam(membership.Id, NewTeam(club, _clock));

        Assert.Equal(OrganisationErrors.OnlyCoachesHaveTeams, result.Error);
        Assert.Empty(membership.TeamIds);
    }

    [Fact]
    public void ACoachCannotBeAssignedToTheSameTeamTwice()
    {
        var club = Club(_clock);
        var coach = club.AddMembership(_user, OrganisationRole.Coach, _clock).Value;
        var team = NewTeam(club, _clock);
        club.AssignCoachToTeam(coach.Id, team);

        var result = club.AssignCoachToTeam(coach.Id, team);

        Assert.Equal(OrganisationErrors.CoachAlreadyAssignedToTeam, result.Error);
        Assert.Single(coach.TeamIds);
    }

    [Fact]
    public void AssignCoachToTeamReportsAMembershipOfAnotherOrganisation()
    {
        var clubA = Club(_clock, name: "Club A");
        var clubB = Club(_clock, name: "Club B");
        var coachInB = clubB.AddMembership(_user, OrganisationRole.Coach, _clock).Value;

        var result = clubA.AssignCoachToTeam(coachInB.Id, NewTeam(clubA, _clock));

        Assert.Equal(OrganisationErrors.MembershipNotFound, result.Error);
    }

    [Fact]
    public void UnassignCoachFromTeamRemovesTheAssignment()
    {
        var club = Club(_clock);
        var coach = club.AddMembership(_user, OrganisationRole.Coach, _clock).Value;
        var team = NewTeam(club, _clock);
        club.AssignCoachToTeam(coach.Id, team);

        var result = club.UnassignCoachFromTeam(coach.Id, team.Id);

        Assert.True(result.IsSuccess);
        Assert.Empty(coach.TeamIds);
    }

    [Fact]
    public void UnassignCoachFromTeamReportsATeamTheCoachIsNotAssignedTo()
    {
        var club = Club(_clock);
        var coach = club.AddMembership(_user, OrganisationRole.Coach, _clock).Value;

        var result = club.UnassignCoachFromTeam(coach.Id, NewTeam(club, _clock).Id);

        Assert.Equal(OrganisationErrors.CoachNotAssignedToTeam, result.Error);
    }
}
