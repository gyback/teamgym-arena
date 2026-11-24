using TeamGym.Arena.Common.Identifiers;
using TeamGym.Arena.Common.Results;
using TeamGym.Arena.Modules.Organisations.Contracts;
using TeamGym.Arena.Modules.Organisations.Domain.Organisations;

namespace TeamGym.Arena.Modules.Organisations.Domain.Gymnasts;

/// <summary>
/// A gymnast, owned by one club and private to it (ADR-0006). Gymnasts are often minors, so the record holds only
/// the personal data that ADR-0007 allows. It is deliberately a class, not a record, so that its personal data never
/// appears in a generated <c>ToString()</c> or in logs.
/// </summary>
internal sealed class Gymnast
{
    public const int NameMaxLength = 100;

    private Gymnast(
        GymnastId id,
        OrganisationId organisationId,
        string givenName,
        string familyName,
        DateOnly dateOfBirth,
        CompetitiveGender competitiveGender)
    {
        Id = id;
        OrganisationId = organisationId;
        GivenName = givenName;
        FamilyName = familyName;
        DateOfBirth = dateOfBirth;
        CompetitiveGender = competitiveGender;
    }

    public GymnastId Id { get; }

    /// <summary>The club that owns the gymnast.</summary>
    public OrganisationId OrganisationId { get; }

    public string GivenName { get; private set; }

    public string FamilyName { get; private set; }

    /// <summary>The date of birth, which decides age-class eligibility.</summary>
    public DateOnly DateOfBirth { get; private set; }

    public CompetitiveGender CompetitiveGender { get; private set; }

    /// <summary>Registers a gymnast in <paramref name="club"/>, which must be an active club.</summary>
    public static Result<Gymnast> Register(
        Organisation club,
        string? givenName,
        string? familyName,
        DateOnly dateOfBirth,
        CompetitiveGender competitiveGender,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(club);
        ArgumentNullException.ThrowIfNull(timeProvider);

        var owner = club.EnsureCanOwnClubResources();
        if (owner.IsFailure)
        {
            return Result.Failure<Gymnast>(owner.Error);
        }

        var details = ValidateDetails(givenName, familyName, dateOfBirth, timeProvider);
        if (details.IsFailure)
        {
            return Result.Failure<Gymnast>(details.Error);
        }

        return Result.Success(new Gymnast(
            StronglyTypedId.New<GymnastId>(timeProvider),
            club.Id,
            details.Value.GivenName,
            details.Value.FamilyName,
            dateOfBirth,
            competitiveGender));
    }

    public bool IsOwnedBy(OrganisationId organisationId) => OrganisationId == organisationId;

    public Result ChangeDetails(
        string? givenName,
        string? familyName,
        DateOnly dateOfBirth,
        CompetitiveGender competitiveGender,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        var details = ValidateDetails(givenName, familyName, dateOfBirth, timeProvider);
        if (details.IsFailure)
        {
            return details;
        }

        (GivenName, FamilyName) = details.Value;
        DateOfBirth = dateOfBirth;
        CompetitiveGender = competitiveGender;

        return Result.Success();
    }

    private static Result<(string GivenName, string FamilyName)> ValidateDetails(
        string? givenName,
        string? familyName,
        DateOnly dateOfBirth,
        TimeProvider timeProvider)
    {
        var trimmedGivenName = givenName?.Trim() ?? string.Empty;
        var trimmedFamilyName = familyName?.Trim() ?? string.Empty;

        if (trimmedGivenName.Length == 0 || trimmedFamilyName.Length == 0)
        {
            return Result.Failure<(string, string)>(GymnastErrors.NameRequired);
        }

        if (trimmedGivenName.Length > NameMaxLength || trimmedFamilyName.Length > NameMaxLength)
        {
            return Result.Failure<(string, string)>(GymnastErrors.NameTooLong);
        }

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        if (dateOfBirth > today)
        {
            return Result.Failure<(string, string)>(GymnastErrors.DateOfBirthInFuture);
        }

        return Result.Success<(string, string)>((trimmedGivenName, trimmedFamilyName));
    }
}
