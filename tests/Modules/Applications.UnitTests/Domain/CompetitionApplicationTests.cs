using Microsoft.Extensions.Time.Testing;
using TeamGym.Arena.Modules.Applications.Domain.CompetitionApplications;
using TeamGym.Arena.Modules.Competitions.Contracts;
using TeamGym.Arena.Modules.Organisations.Contracts;

namespace TeamGym.Arena.Modules.Applications.UnitTests.Domain;

/// <summary>
/// The two-party visibility of an application (ADR-0006): club A applies to organiser C's competition, and club B
/// must never see it.
/// </summary>
public sealed class CompetitionApplicationTests
{
    private static readonly DateTimeOffset s_now = new(2027, 3, 14, 9, 30, 0, TimeSpan.Zero);

    private readonly FakeTimeProvider _clock = new(s_now);

    private readonly OrganisationId _clubA = NewOrganisationId();

    private readonly OrganisationId _clubB = NewOrganisationId();

    private readonly OrganisationId _organiserC = NewOrganisationId();

    [Fact]
    public void StartMakesADraftForAPublishedCompetition()
    {
        var competition = Competition(isPublished: true);

        var result = CompetitionApplication.Start(competition, _clubA, _clock);

        var application = result.Value;
        Assert.Equal(competition.Id, application.CompetitionId);
        Assert.Equal(_organiserC, application.OrganiserOrganisationId);
        Assert.Equal(_clubA, application.ApplyingOrganisationId);
        Assert.Equal(CompetitionApplicationStatus.Draft, application.Status);
        Assert.Equal(7, application.Id.Value.Version);
    }

    [Fact]
    public void ADraftCompetitionIsNotFoundForAnotherClub()
    {
        var result = CompetitionApplication.Start(Competition(isPublished: false), _clubA, _clock);

        Assert.Equal(CompetitionApplicationErrors.CompetitionNotFound, result.Error);
    }

    [Fact]
    public void TheOrganiserCannotApplyToItsOwnDraftCompetition()
    {
        var result = CompetitionApplication.Start(Competition(isPublished: false), _organiserC, _clock);

        Assert.Equal(CompetitionApplicationErrors.CompetitionNotPublished, result.Error);
    }

    [Fact]
    public void TheOrganiserMayApplyToItsOwnPublishedCompetition()
    {
        var application = CompetitionApplication.Start(Competition(isPublished: true), _organiserC, _clock).Value;

        Assert.True(application.IsVisibleTo(_organiserC));
        Assert.False(application.IsVisibleTo(_clubB));
    }

    [Fact]
    public void ADraftApplicationIsVisibleOnlyToTheApplyingClub()
    {
        var application = CompetitionApplication.Start(Competition(isPublished: true), _clubA, _clock).Value;

        Assert.True(application.IsVisibleTo(_clubA));
        Assert.False(application.IsVisibleTo(_organiserC));
        Assert.False(application.IsVisibleTo(_clubB));
    }

    [Fact]
    public void ASubmittedApplicationIsVisibleToTheApplyingClubAndTheOrganiserOnly()
    {
        var application = CompetitionApplication.Start(Competition(isPublished: true), _clubA, _clock).Value;

        var result = application.Submit();

        Assert.True(result.IsSuccess);
        Assert.Equal(CompetitionApplicationStatus.Submitted, application.Status);
        Assert.True(application.IsVisibleTo(_clubA));
        Assert.True(application.IsVisibleTo(_organiserC));
        Assert.False(application.IsVisibleTo(_clubB));
    }

    [Fact]
    public void SubmitTwiceFails()
    {
        var application = CompetitionApplication.Start(Competition(isPublished: true), _clubA, _clock).Value;
        application.Submit();

        var result = application.Submit();

        Assert.Equal(CompetitionApplicationErrors.AlreadySubmitted, result.Error);
    }

    private static OrganisationId NewOrganisationId() => OrganisationId.From(Guid.CreateVersion7(s_now));

    private CompetitionSummary Competition(bool isPublished) => new(
        CompetitionId.From(Guid.CreateVersion7(s_now)), _organiserC, "Spring Cup", isPublished);
}
