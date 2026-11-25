using TeamGym.Arena.Common.Results;

namespace TeamGym.Arena.Modules.Organisations.Domain.Organisations;

internal static class OrganisationErrors
{
    public static readonly Error NameRequired = Error.Validation(
        "Organisations.NameRequired", "An organisation needs a name.");

    public static readonly Error NameTooLong = Error.Validation(
        "Organisations.NameTooLong", $"An organisation name is at most {Organisation.NameMaxLength} characters.");

    public static readonly Error ShortNameTooLong = Error.Validation(
        "Organisations.ShortNameTooLong", $"An organisation short name is at most {Organisation.ShortNameMaxLength} characters.");

    public static readonly Error InvalidCountryCode = Error.Validation(
        "Organisations.InvalidCountryCode", "The country code must be an ISO 3166-1 alpha-2 code, such as SE.");

    public static readonly Error InvalidParentType = Error.Validation(
        "Organisations.InvalidParentType",
        "A club belongs to a region or a federation, a region to a federation, and a federation to a federation.");

    public static readonly Error ParentArchived = Error.Conflict(
        "Organisations.ParentArchived", "An archived organisation cannot get new member organisations.");

    public static readonly Error HierarchyCycle = Error.Conflict(
        "Organisations.HierarchyCycle", "An organisation cannot belong to itself or to one of its own descendants.");

    public static readonly Error Archived = Error.Conflict(
        "Organisations.Archived", "The organisation is archived and cannot be changed.");

    public static readonly Error NotAClub = Error.Validation(
        "Organisations.NotAClub", "Only a club owns gymnasts and teams.");

    public static readonly Error DuplicateMembership = Error.Conflict(
        "Organisations.DuplicateMembership", "The user already has this role in the organisation.");

    public static readonly Error MembershipNotFound = Error.NotFound(
        "Organisations.MembershipNotFound", "The organisation has no such membership.");

    public static readonly Error OnlyCoachesHaveTeams = Error.Validation(
        "Organisations.OnlyCoachesHaveTeams", "Only a coach membership can be assigned to teams.");

    public static readonly Error CoachAlreadyAssignedToTeam = Error.Conflict(
        "Organisations.CoachAlreadyAssignedToTeam", "The coach is already assigned to the team.");

    public static readonly Error CoachNotAssignedToTeam = Error.NotFound(
        "Organisations.CoachNotAssignedToTeam", "The coach is not assigned to the team.");
}
