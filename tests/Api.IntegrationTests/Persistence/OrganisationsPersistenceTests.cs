using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Npgsql;
using TeamGym.Arena.Api.IntegrationTests.Infrastructure;
using TeamGym.Arena.Common;
using TeamGym.Arena.Modules.Competitions.Contracts;
using TeamGym.Arena.Modules.Organisations.Contracts;
using TeamGym.Arena.Modules.Organisations.Domain.Gymnasts;
using TeamGym.Arena.Modules.Organisations.Domain.Organisations;
using TeamGym.Arena.Modules.Organisations.Domain.Teams;
using TeamGym.Arena.Modules.Organisations.Persistence;
using TeamGym.Arena.Modules.Users.Contracts;

namespace TeamGym.Arena.Api.IntegrationTests.Persistence;

/// <summary>
/// The Organisations module's mappings against real PostgreSQL: aggregates load whole, and the database enforces
/// the constraints and the optimistic concurrency of ADR-0007. All data is synthetic.
/// </summary>
public sealed class OrganisationsPersistenceTests(ApiFactory factory) : IClassFixture<ApiFactory>, IAsyncLifetime
{
    // Whole seconds, because PostgreSQL stores timestamps to the microsecond.
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2027, 3, 14, 9, 30, 0, TimeSpan.Zero));

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync() => await factory.Database.ResetAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task OrganisationLoadsWithItsHierarchyMembershipsAndCoachAssignments()
    {
        var federation = Organisation.Create("Test Federation", "TF", OrganisationType.Federation, "se", null, _clock).Value;
        var club = Organisation.Create("Test Club", "TC", OrganisationType.Club, "se", federation, _clock).Value;
        var ruleSetId = RuleSetId.From(Guid.CreateVersion7());
        Assert.True(club.SetDefaultRuleSet(ruleSetId).IsSuccess);
        var team = Team.Create(club, "Senior Team", Section.Mixed, _clock).Value;
        var userId = UserId.From(Guid.CreateVersion7());
        var coach = club.AddMembership(userId, OrganisationRole.Coach, _clock).Value;
        Assert.True(club.AssignCoachToTeam(coach.Id, team).IsSuccess);
        await AddAsync(federation, club, team);

        await using var scope = factory.Services.CreateAsyncScope();
        var loaded = await Context(scope).Organisations.SingleAsync(organisation => organisation.Id == club.Id, CancellationToken);

        Assert.Equal("Test Club", loaded.Name);
        Assert.Equal("TC", loaded.ShortName);
        Assert.Equal(OrganisationType.Club, loaded.Type);
        Assert.Equal("SE", loaded.CountryCode.Value);
        Assert.Equal(federation.Id, loaded.ParentOrganisationId);
        Assert.Equal(ruleSetId, loaded.DefaultRuleSetId);
        Assert.True(loaded.IsActive);
        var membership = Assert.Single(loaded.Memberships);
        Assert.Equal(coach.Id, membership.Id);
        Assert.Equal(userId, membership.UserId);
        Assert.Equal(OrganisationRole.Coach, membership.Role);
        Assert.Equal(_clock.GetUtcNow(), membership.JoinedAt);
        Assert.Equal([team.Id], membership.TeamIds);
    }

    [Fact]
    public async Task TeamLoadsWithItsGymnastsAndSavesARemovedGymnast()
    {
        var club = Club();
        var first = NewGymnast(club);
        var second = NewGymnast(club);
        var team = Team.Create(club, "Senior Team", Section.Mixed, _clock).Value;
        Assert.True(team.AddGymnast(first).IsSuccess);
        Assert.True(team.AddGymnast(second).IsSuccess);
        await AddAsync(club, first, second, team);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var context = Context(scope);
            var loaded = await context.Teams.SingleAsync(candidate => candidate.Id == team.Id, CancellationToken);
            Assert.Equal(new HashSet<GymnastId> { first.Id, second.Id }, loaded.GymnastIds.ToHashSet());

            Assert.True(loaded.RemoveGymnast(first.Id).IsSuccess);
            await context.SaveChangesAsync(CancellationToken);
        }

        Assert.Equal([second.Id], (await LoadTeamAsync(team.Id)).GymnastIds);
    }

    [Fact]
    public async Task DeletingAGymnastRemovesItFromItsTeams()
    {
        var club = Club();
        var gymnast = NewGymnast(club);
        var team = Team.Create(club, "Senior Team", Section.Women, _clock).Value;
        Assert.True(team.AddGymnast(gymnast).IsSuccess);
        await AddAsync(club, gymnast, team);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var context = Context(scope);
            context.Gymnasts.Remove(await context.Gymnasts.SingleAsync(candidate => candidate.Id == gymnast.Id, CancellationToken));
            await context.SaveChangesAsync(CancellationToken);
        }

        Assert.Empty((await LoadTeamAsync(team.Id)).GymnastIds);
    }

    [Fact]
    public async Task SavingAStaleAggregateThrowsAConcurrencyException()
    {
        var club = Club();
        await AddAsync(club);

        await using var firstScope = factory.Services.CreateAsyncScope();
        await using var secondScope = factory.Services.CreateAsyncScope();
        var firstContext = Context(firstScope);
        var secondContext = Context(secondScope);
        var first = await firstContext.Organisations.SingleAsync(candidate => candidate.Id == club.Id, CancellationToken);
        var second = await secondContext.Organisations.SingleAsync(candidate => candidate.Id == club.Id, CancellationToken);

        Assert.True(first.Rename("First Name", null).IsSuccess);
        await firstContext.SaveChangesAsync(CancellationToken);
        Assert.True(second.Rename("Second Name", null).IsSuccess);

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => secondContext.SaveChangesAsync(CancellationToken));
    }

    [Fact]
    public async Task DatabaseRejectsTheSameRoleTwiceEvenWhenTwoRequestsRace()
    {
        var club = Club();
        await AddAsync(club);
        var userId = UserId.From(Guid.CreateVersion7());

        // Each request loads the organisation before the other saves, so the aggregate's own check passes in both.
        await using var firstScope = factory.Services.CreateAsyncScope();
        await using var secondScope = factory.Services.CreateAsyncScope();
        var firstContext = Context(firstScope);
        var secondContext = Context(secondScope);
        var first = await firstContext.Organisations.SingleAsync(candidate => candidate.Id == club.Id, CancellationToken);
        var second = await secondContext.Organisations.SingleAsync(candidate => candidate.Id == club.Id, CancellationToken);
        Assert.True(first.AddMembership(userId, OrganisationRole.Admin, _clock).IsSuccess);
        Assert.True(second.AddMembership(userId, OrganisationRole.Admin, _clock).IsSuccess);

        await firstContext.SaveChangesAsync(CancellationToken);
        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => secondContext.SaveChangesAsync(CancellationToken));

        Assert.Equal(PostgresErrorCodes.UniqueViolation, Assert.IsType<PostgresException>(exception.InnerException).SqlState);
    }

    [Fact]
    public async Task EnumsAreStoredAsTheirNames()
    {
        var club = Club();
        await AddAsync(club);

        await using var connection = await factory.Database.OpenConnectionAsync(CancellationToken);
        await using var command = new NpgsqlCommand(
            "SELECT type, status, country_code FROM organisations.organisations", connection);
        await using var reader = await command.ExecuteReaderAsync(CancellationToken);

        Assert.True(await reader.ReadAsync(CancellationToken));
        Assert.Equal(("Club", "Active", "SE"), (reader.GetString(0), reader.GetString(1), reader.GetString(2)));
    }

    private Organisation Club() =>
        Organisation.Create("Test Club", null, OrganisationType.Club, "SE", null, _clock).Value;

    private Gymnast NewGymnast(Organisation club) =>
        Gymnast.Register(club, "Alex", "Example", new DateOnly(2012, 5, 1), CompetitiveGender.Female, _clock).Value;

    private static OrganisationsDbContext Context(AsyncServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<OrganisationsDbContext>();

    private async Task AddAsync(params object[] entities)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var context = Context(scope);
        context.AddRange(entities);
        await context.SaveChangesAsync(CancellationToken);
    }

    private async Task<Team> LoadTeamAsync(TeamId teamId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await Context(scope).Teams.SingleAsync(team => team.Id == teamId, CancellationToken);
    }
}
