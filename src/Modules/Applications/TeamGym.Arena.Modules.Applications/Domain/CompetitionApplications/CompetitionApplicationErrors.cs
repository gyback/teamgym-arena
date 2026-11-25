using TeamGym.Arena.Common.Results;

namespace TeamGym.Arena.Modules.Applications.Domain.CompetitionApplications;

internal static class CompetitionApplicationErrors
{
    public static readonly Error CompetitionNotFound = Error.NotFound(
        "Applications.CompetitionNotFound", "There is no such competition.");

    public static readonly Error CompetitionNotPublished = Error.Conflict(
        "Applications.CompetitionNotPublished", "A competition takes applications only once it is published.");

    public static readonly Error AlreadySubmitted = Error.Conflict(
        "Applications.AlreadySubmitted", "The application is already submitted.");
}
