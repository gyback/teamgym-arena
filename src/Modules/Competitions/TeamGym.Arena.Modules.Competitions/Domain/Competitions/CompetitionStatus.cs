namespace TeamGym.Arena.Modules.Competitions.Domain.Competitions;

/// <summary>
/// Where a competition is in its life cycle. Only a <see cref="Draft"/> is private to its organiser (ADR-0006);
/// later stages, such as opening and closing applications, are added with their use cases.
/// </summary>
internal enum CompetitionStatus
{
    Draft,
    Published,
}
