using Microsoft.Extensions.Time.Testing;
using TeamGym.Arena.Common;
using TeamGym.Arena.Modules.Organisations.Contracts;
using TeamGym.Arena.Modules.Organisations.Domain.Gymnasts;
using TeamGym.Arena.Modules.Organisations.Domain.Organisations;
using TeamGym.Arena.Modules.Organisations.Domain.Teams;

namespace TeamGym.Arena.Modules.Organisations.UnitTests.Domain;

/// <summary>Builds valid domain objects with synthetic data, so each test states only what it is about.</summary>
internal static class DomainFixture
{
    public static readonly DateTimeOffset Now = new(2027, 3, 14, 9, 30, 0, TimeSpan.Zero);

    public static FakeTimeProvider Clock() => new(Now);

    public static Organisation Federation(TimeProvider clock, Organisation? parent = null) =>
        Organisation.Create("Test Federation", "TF", OrganisationType.Federation, "SE", parent, clock).Value;

    public static Organisation Region(TimeProvider clock, Organisation? parent = null) =>
        Organisation.Create("Test Region", null, OrganisationType.Region, "SE", parent, clock).Value;

    public static Organisation Club(TimeProvider clock, string name = "Test Club", Organisation? parent = null) =>
        Organisation.Create(name, null, OrganisationType.Club, "SE", parent, clock).Value;

    public static Team NewTeam(Organisation club, TimeProvider clock) =>
        Team.Create(club, "Senior Team", Section.Mixed, clock).Value;

    public static Gymnast NewGymnast(Organisation club, TimeProvider clock) =>
        Gymnast.Register(
            club, "Alex", "Example", new DateOnly(2012, 5, 1), CompetitiveGender.Female, clock).Value;
}
