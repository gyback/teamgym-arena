namespace TeamGym.Arena.Modules.Organisations.Domain.Organisations;

/// <summary>
/// The role a member holds in an organisation (ADR-0006). Endpoints and handlers check the permissions that a role
/// grants, never the role itself (ADR-0008).
/// </summary>
internal enum OrganisationRole
{
    Admin,
    Coach,
    CompetitionOrganiser,
}
