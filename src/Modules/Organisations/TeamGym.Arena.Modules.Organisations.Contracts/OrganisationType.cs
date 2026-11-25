namespace TeamGym.Arena.Modules.Organisations.Contracts;

/// <summary>
/// The kind of an organisation (ADR-0006). Clubs own gymnasts and teams; any type of organisation can host a
/// competition.
/// </summary>
public enum OrganisationType
{
    Club,
    Region,
    Federation,
}
