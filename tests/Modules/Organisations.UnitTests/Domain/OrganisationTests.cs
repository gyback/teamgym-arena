using TeamGym.Arena.Modules.Competitions.Contracts;
using TeamGym.Arena.Modules.Organisations.Contracts;
using TeamGym.Arena.Modules.Organisations.Domain.Organisations;
using static TeamGym.Arena.Modules.Organisations.UnitTests.Domain.DomainFixture;

namespace TeamGym.Arena.Modules.Organisations.UnitTests.Domain;

public sealed class OrganisationTests
{
    private readonly TimeProvider _clock = Clock();

    [Fact]
    public void CreateMakesAnActiveTopLevelOrganisationWithTrimmedNames()
    {
        var result = Organisation.Create("  GF Test  ", " GFT ", OrganisationType.Club, "se", parent: null, _clock);

        Assert.True(result.IsSuccess);
        var club = result.Value;
        Assert.Equal("GF Test", club.Name);
        Assert.Equal("GFT", club.ShortName);
        Assert.Equal(OrganisationType.Club, club.Type);
        Assert.Equal("SE", club.CountryCode.Value);
        Assert.Null(club.ParentOrganisationId);
        Assert.Null(club.DefaultRuleSetId);
        Assert.True(club.IsActive);
        Assert.Equal(7, club.Id.Value.Version);
    }

    [Fact]
    public void CreateTreatsABlankShortNameAsNone()
    {
        var club = Organisation.Create("GF Test", "   ", OrganisationType.Club, "SE", parent: null, _clock).Value;

        Assert.Null(club.ShortName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateRequiresAName(string? name)
    {
        var result = Organisation.Create(name, null, OrganisationType.Club, "SE", parent: null, _clock);

        Assert.Equal(OrganisationErrors.NameRequired, result.Error);
    }

    [Fact]
    public void CreateRejectsATooLongName()
    {
        var name = new string('a', OrganisationErrors.NameMaxLength + 1);

        var result = Organisation.Create(name, null, OrganisationType.Club, "SE", parent: null, _clock);

        Assert.Equal(OrganisationErrors.NameTooLong, result.Error);
    }

    [Fact]
    public void CreateRejectsATooLongShortName()
    {
        var shortName = new string('a', OrganisationErrors.ShortNameMaxLength + 1);

        var result = Organisation.Create("GF Test", shortName, OrganisationType.Club, "SE", parent: null, _clock);

        Assert.Equal(OrganisationErrors.ShortNameTooLong, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("S")]
    [InlineData("SWE")]
    [InlineData("S1")]
    [InlineData("ÅÄ")]
    public void CreateRejectsAnInvalidCountryCode(string? countryCode)
    {
        var result = Organisation.Create("GF Test", null, OrganisationType.Club, countryCode, parent: null, _clock);

        Assert.Equal(OrganisationErrors.InvalidCountryCode, result.Error);
    }

    [Fact]
    public void CreateAcceptsTheEuropeanCountryCode()
    {
        var result = Organisation.Create(
            "Test European Federation", null, OrganisationType.Federation, "EU", parent: null, _clock);

        Assert.Equal("EU", result.Value.CountryCode.Value);
    }

    [Theory]
    [InlineData(OrganisationType.Federation, OrganisationType.Federation)]
    [InlineData(OrganisationType.Federation, OrganisationType.Region)]
    [InlineData(OrganisationType.Federation, OrganisationType.Club)]
    [InlineData(OrganisationType.Region, OrganisationType.Club)]
    public void CreateAcceptsAParentHigherInTheHierarchy(OrganisationType parentType, OrganisationType childType)
    {
        var parent = Organisation.Create("Parent", null, parentType, "SE", parent: null, _clock).Value;

        var result = Organisation.Create("Child", null, childType, "SE", parent, _clock);

        Assert.Equal(parent.Id, result.Value.ParentOrganisationId);
    }

    [Theory]
    [InlineData(OrganisationType.Club, OrganisationType.Club)]
    [InlineData(OrganisationType.Club, OrganisationType.Region)]
    [InlineData(OrganisationType.Club, OrganisationType.Federation)]
    [InlineData(OrganisationType.Region, OrganisationType.Region)]
    [InlineData(OrganisationType.Region, OrganisationType.Federation)]
    public void CreateRejectsAParentThatCannotHoldTheType(OrganisationType parentType, OrganisationType childType)
    {
        var parent = Organisation.Create("Parent", null, parentType, "SE", parent: null, _clock).Value;

        var result = Organisation.Create("Child", null, childType, "SE", parent, _clock);

        Assert.Equal(OrganisationErrors.InvalidParentType, result.Error);
    }

    [Fact]
    public void CreateRejectsAnArchivedParent()
    {
        var region = Region(_clock);
        region.Archive();

        var result = Organisation.Create("Child", null, OrganisationType.Club, "SE", region, _clock);

        Assert.Equal(OrganisationErrors.ParentArchived, result.Error);
    }

    [Fact]
    public void ChangeParentLinksAClubToARegion()
    {
        var club = Club(_clock);
        var region = Region(_clock);

        var result = club.ChangeParent(region, ancestorsOfParent: []);

        Assert.True(result.IsSuccess);
        Assert.Equal(region.Id, club.ParentOrganisationId);
    }

    [Fact]
    public void ChangeParentToNoneMakesTheOrganisationTopLevel()
    {
        var club = Club(_clock, parent: Region(_clock));

        var result = club.ChangeParent(parent: null, ancestorsOfParent: []);

        Assert.True(result.IsSuccess);
        Assert.Null(club.ParentOrganisationId);
    }

    [Fact]
    public void ChangeParentRejectsTheOrganisationItself()
    {
        var federation = Federation(_clock);

        var result = federation.ChangeParent(federation, ancestorsOfParent: []);

        Assert.Equal(OrganisationErrors.HierarchyCycle, result.Error);
    }

    [Fact]
    public void ChangeParentRejectsADescendant()
    {
        var european = Federation(_clock);
        var national = Federation(_clock, parent: european);
        var regionalFederation = Federation(_clock, parent: national);

        var result = european.ChangeParent(regionalFederation, ancestorsOfParent: [national.Id, european.Id]);

        Assert.Equal(OrganisationErrors.HierarchyCycle, result.Error);
        Assert.Null(european.ParentOrganisationId);
    }

    [Fact]
    public void ChangeParentRejectsAParentThatCannotHoldTheType()
    {
        var region = Region(_clock);
        var club = Club(_clock);

        var result = region.ChangeParent(club, ancestorsOfParent: []);

        Assert.Equal(OrganisationErrors.InvalidParentType, result.Error);
        Assert.Null(region.ParentOrganisationId);
    }

    [Fact]
    public void RenameReplacesTheNames()
    {
        var club = Club(_clock);

        var result = club.Rename(" GF Renamed ", "GFR");

        Assert.True(result.IsSuccess);
        Assert.Equal("GF Renamed", club.Name);
        Assert.Equal("GFR", club.ShortName);
    }

    [Fact]
    public void RenameKeepsTheNamesWhenTheNewNameIsInvalid()
    {
        var club = Club(_clock, name: "GF Test");

        var result = club.Rename("", "GFR");

        Assert.Equal(OrganisationErrors.NameRequired, result.Error);
        Assert.Equal("GF Test", club.Name);
        Assert.Null(club.ShortName);
    }

    [Fact]
    public void SetDefaultRuleSetStoresAndClearsTheDefault()
    {
        var club = Club(_clock);
        var ruleSetId = RuleSetId.From(Guid.CreateVersion7());

        club.SetDefaultRuleSet(ruleSetId);
        Assert.Equal(ruleSetId, club.DefaultRuleSetId);

        club.SetDefaultRuleSet(null);
        Assert.Null(club.DefaultRuleSetId);
    }

    [Fact]
    public void ArchiveDeactivatesTheOrganisation()
    {
        var club = Club(_clock);

        var result = club.Archive();

        Assert.True(result.IsSuccess);
        Assert.False(club.IsActive);
        Assert.Equal(OrganisationStatus.Archived, club.Status);
    }

    [Fact]
    public void AnArchivedOrganisationCannotBeChanged()
    {
        var club = Club(_clock, name: "GF Test");
        club.Archive();

        Assert.Equal(OrganisationErrors.Archived, club.Archive().Error);
        Assert.Equal(OrganisationErrors.Archived, club.Rename("GF Renamed", null).Error);
        Assert.Equal(OrganisationErrors.Archived, club.ChangeParent(Region(_clock), []).Error);
        Assert.Equal(OrganisationErrors.Archived, club.SetDefaultRuleSet(RuleSetId.From(Guid.CreateVersion7())).Error);
        Assert.Equal("GF Test", club.Name);
        Assert.Null(club.ParentOrganisationId);
        Assert.Null(club.DefaultRuleSetId);
    }
}
