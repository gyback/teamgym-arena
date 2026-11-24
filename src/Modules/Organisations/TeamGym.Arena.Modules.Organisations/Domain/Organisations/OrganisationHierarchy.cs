using TeamGym.Arena.Modules.Organisations.Contracts;

namespace TeamGym.Arena.Modules.Organisations.Domain.Organisations;

/// <summary>
/// Which organisation types may sit above which (ADR-0006): a club belongs to a region or a federation, a region
/// belongs to a federation, and a federation may belong to a wider federation, such as a national federation
/// under European Gymnastics.
/// </summary>
internal static class OrganisationHierarchy
{
    public static bool CanBeParentOf(OrganisationType parent, OrganisationType child) => (parent, child) switch
    {
        (OrganisationType.Federation, _) => true,
        (OrganisationType.Region, OrganisationType.Club) => true,
        _ => false,
    };
}
