namespace TeamGym.Arena.Modules.Applications.Domain.CompetitionApplications;

/// <summary>
/// Where an application is in its life cycle. Only a <see cref="Draft"/> is private to the applying organisation
/// (ADR-0006); the organiser's review stages are added with their use cases.
/// </summary>
internal enum CompetitionApplicationStatus
{
    Draft,
    Submitted,
}
