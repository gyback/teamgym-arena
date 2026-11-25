using TeamGym.Arena.Common.Identifiers;
using TeamGym.Arena.Common.Results;
using TeamGym.Arena.Modules.Applications.Contracts;
using TeamGym.Arena.Modules.Competitions.Contracts;
using TeamGym.Arena.Modules.Organisations.Contracts;

namespace TeamGym.Arena.Modules.Applications.Domain.CompetitionApplications;

/// <summary>
/// A club's application to a competition: a record shared between exactly two parties, the applying organisation
/// and the competition's organiser (ADR-0006). While it is a draft only the applying organisation sees it; once
/// submitted, the organiser sees it too. No other organisation ever does.
/// </summary>
internal sealed class CompetitionApplication
{
    private CompetitionApplication(
        CompetitionApplicationId id,
        CompetitionId competitionId,
        OrganisationId organiserOrganisationId,
        OrganisationId applyingOrganisationId)
    {
        Id = id;
        CompetitionId = competitionId;
        OrganiserOrganisationId = organiserOrganisationId;
        ApplyingOrganisationId = applyingOrganisationId;
        Status = CompetitionApplicationStatus.Draft;
    }

    public CompetitionApplicationId Id { get; }

    public CompetitionId CompetitionId { get; }

    /// <summary>
    /// The competition's organiser, copied from the competition so that the visibility rule needs no call to the
    /// Competitions module. A competition's organiser never changes.
    /// </summary>
    public OrganisationId OrganiserOrganisationId { get; }

    /// <summary>The club that applies, and that alone may change the application.</summary>
    public OrganisationId ApplyingOrganisationId { get; }

    public CompetitionApplicationStatus Status { get; private set; }

    public bool IsDraft => Status == CompetitionApplicationStatus.Draft;

    /// <summary>
    /// Starts a draft application by <paramref name="applyingOrganisationId"/> to a published competition. The
    /// organiser may apply to its own competition. A draft competition is reported as not found to anyone but its
    /// organiser, because nobody else may see it.
    /// </summary>
    public static Result<CompetitionApplication> Start(
        CompetitionSummary competition,
        OrganisationId applyingOrganisationId,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(competition);
        ArgumentNullException.ThrowIfNull(timeProvider);

        if (!competition.IsPublished)
        {
            return Result.Failure<CompetitionApplication>(
                competition.OrganiserOrganisationId == applyingOrganisationId
                    ? CompetitionApplicationErrors.CompetitionNotPublished
                    : CompetitionApplicationErrors.CompetitionNotFound);
        }

        return Result.Success(new CompetitionApplication(
            StronglyTypedId.New<CompetitionApplicationId>(timeProvider),
            competition.Id,
            competition.OrganiserOrganisationId,
            applyingOrganisationId));
    }

    /// <summary>
    /// Whether <paramref name="organisationId"/> may see the application: the applying organisation always, and the
    /// organiser once it is submitted (ADR-0006).
    /// </summary>
    public bool IsVisibleTo(OrganisationId organisationId) =>
        organisationId == ApplyingOrganisationId || (!IsDraft && organisationId == OrganiserOrganisationId);

    /// <summary>Submits the draft to the organiser.</summary>
    public Result Submit()
    {
        if (!IsDraft)
        {
            return Result.Failure(CompetitionApplicationErrors.AlreadySubmitted);
        }

        Status = CompetitionApplicationStatus.Submitted;

        return Result.Success();
    }
}
