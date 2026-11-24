namespace TeamGym.Arena.Modules.Organisations.Contracts;

/// <summary>
/// The public summary of an organisation, which every authenticated user may see (ADR-0006).
/// </summary>
/// <param name="CountryCode">An ISO 3166-1 alpha-2 code in upper case, such as <c>SE</c>.</param>
/// <param name="IsActive"><see langword="false"/> when the organisation is archived.</param>
public sealed record OrganisationSummary(
    OrganisationId Id,
    string Name,
    string? ShortName,
    OrganisationType Type,
    string CountryCode,
    OrganisationId? ParentOrganisationId,
    bool IsActive);
