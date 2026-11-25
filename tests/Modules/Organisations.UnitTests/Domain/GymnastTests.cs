using TeamGym.Arena.Modules.Organisations.Domain.Gymnasts;
using TeamGym.Arena.Modules.Organisations.Domain.Organisations;
using static TeamGym.Arena.Modules.Organisations.UnitTests.Domain.DomainFixture;

namespace TeamGym.Arena.Modules.Organisations.UnitTests.Domain;

public sealed class GymnastTests
{
    private static readonly DateOnly s_dateOfBirth = new(2012, 5, 1);

    private readonly TimeProvider _clock = Clock();

    [Fact]
    public void RegisterAddsAGymnastToTheClub()
    {
        var club = Club(_clock);

        var result = Gymnast.Register(club, " Alex ", " Example ", s_dateOfBirth, CompetitiveGender.Male, _clock);

        var gymnast = result.Value;
        Assert.Equal(club.Id, gymnast.OrganisationId);
        Assert.True(gymnast.IsOwnedBy(club.Id));
        Assert.Equal("Alex", gymnast.GivenName);
        Assert.Equal("Example", gymnast.FamilyName);
        Assert.Equal(s_dateOfBirth, gymnast.DateOfBirth);
        Assert.Equal(CompetitiveGender.Male, gymnast.CompetitiveGender);
    }

    [Fact]
    public void AGymnastIsNotOwnedByAnotherClub()
    {
        var gymnast = NewGymnast(Club(_clock, name: "Club A"), _clock);

        Assert.False(gymnast.IsOwnedBy(Club(_clock, name: "Club B").Id));
    }

    [Fact]
    public void OnlyAClubOwnsGymnasts()
    {
        var result = Gymnast.Register(
            Federation(_clock), "Alex", "Example", s_dateOfBirth, CompetitiveGender.Female, _clock);

        Assert.Equal(OrganisationErrors.NotAClub, result.Error);
    }

    [Fact]
    public void AnArchivedClubCannotRegisterGymnasts()
    {
        var club = Club(_clock);
        club.Archive();

        var result = Gymnast.Register(club, "Alex", "Example", s_dateOfBirth, CompetitiveGender.Female, _clock);

        Assert.Equal(OrganisationErrors.Archived, result.Error);
    }

    [Theory]
    [InlineData(null, "Example")]
    [InlineData("Alex", null)]
    [InlineData(" ", "Example")]
    [InlineData("Alex", " ")]
    public void RegisterRequiresBothNames(string? givenName, string? familyName)
    {
        var result = Gymnast.Register(
            Club(_clock), givenName, familyName, s_dateOfBirth, CompetitiveGender.Female, _clock);

        Assert.Equal(GymnastErrors.NameRequired, result.Error);
    }

    [Fact]
    public void RegisterRejectsATooLongName()
    {
        var result = Gymnast.Register(
            Club(_clock),
            "Alex",
            new string('a', Gymnast.NameMaxLength + 1),
            s_dateOfBirth,
            CompetitiveGender.Female,
            _clock);

        Assert.Equal(GymnastErrors.NameTooLong, result.Error);
    }

    [Fact]
    public void RegisterAcceptsADateOfBirthOfToday()
    {
        var today = DateOnly.FromDateTime(Now.UtcDateTime);

        var result = Gymnast.Register(Club(_clock), "Alex", "Example", today, CompetitiveGender.Female, _clock);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void RegisterRejectsADateOfBirthInTheFuture()
    {
        var tomorrow = DateOnly.FromDateTime(Now.UtcDateTime).AddDays(1);

        var result = Gymnast.Register(Club(_clock), "Alex", "Example", tomorrow, CompetitiveGender.Female, _clock);

        Assert.Equal(GymnastErrors.DateOfBirthInFuture, result.Error);
    }

    [Fact]
    public void ChangeDetailsReplacesTheDetails()
    {
        var gymnast = NewGymnast(Club(_clock), _clock);
        var newDateOfBirth = new DateOnly(2011, 12, 24);

        var result = gymnast.ChangeDetails("Sam", "Sample", newDateOfBirth, CompetitiveGender.Male, _clock);

        Assert.True(result.IsSuccess);
        Assert.Equal("Sam", gymnast.GivenName);
        Assert.Equal("Sample", gymnast.FamilyName);
        Assert.Equal(newDateOfBirth, gymnast.DateOfBirth);
        Assert.Equal(CompetitiveGender.Male, gymnast.CompetitiveGender);
    }

    [Fact]
    public void ChangeDetailsKeepsTheDetailsWhenTheyAreInvalid()
    {
        var gymnast = NewGymnast(Club(_clock), _clock);

        var result = gymnast.ChangeDetails("", "Sample", s_dateOfBirth, CompetitiveGender.Male, _clock);

        Assert.Equal(GymnastErrors.NameRequired, result.Error);
        Assert.Equal("Alex", gymnast.GivenName);
        Assert.Equal("Example", gymnast.FamilyName);
        Assert.Equal(CompetitiveGender.Female, gymnast.CompetitiveGender);
    }

    [Fact]
    public void ToStringDoesNotRevealPersonalData()
    {
        var gymnast = NewGymnast(Club(_clock), _clock);

        var text = gymnast.ToString();

        Assert.DoesNotContain("Alex", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Example", text, StringComparison.Ordinal);
    }
}
