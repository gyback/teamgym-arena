using TeamGym.Arena.Common;
using TeamGym.Arena.Common.Identifiers;
using TeamGym.Arena.Common.Results;
using TeamGym.Arena.Modules.Organisations.Contracts;
using TeamGym.Arena.Modules.Organisations.Domain.Gymnasts;
using TeamGym.Arena.Modules.Organisations.Domain.Organisations;

namespace TeamGym.Arena.Modules.Organisations.Domain.Teams;

/// <summary>
/// A team: the competing unit in TeamGym, owned by one club (ADR-0006). A team holds only gymnasts of its own
/// club. Squad size and gender composition depend on the competition's rule set, so they are checked when the
/// team enters a competition, not here.
/// </summary>
internal sealed class Team
{
    public const int NameMaxLength = 100;

    private readonly HashSet<GymnastId> _gymnastIds = [];

    private Team(TeamId id, OrganisationId organisationId, string name, Section section)
    {
        Id = id;
        OrganisationId = organisationId;
        Name = name;
        Section = section;
    }

    public TeamId Id { get; }

    /// <summary>The club that owns the team.</summary>
    public OrganisationId OrganisationId { get; }

    public string Name { get; private set; }

    public Section Section { get; private set; }

    public IReadOnlyCollection<GymnastId> GymnastIds => _gymnastIds;

    /// <summary>Creates a team owned by <paramref name="club"/>, which must be an active club.</summary>
    public static Result<Team> Create(Organisation club, string? name, Section section, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(club);
        ArgumentNullException.ThrowIfNull(timeProvider);

        var owner = club.EnsureCanOwnClubResources();
        if (owner.IsFailure)
        {
            return Result.Failure<Team>(owner.Error);
        }

        var validName = ValidateName(name);
        if (validName.IsFailure)
        {
            return Result.Failure<Team>(validName.Error);
        }

        return Result.Success(new Team(StronglyTypedId.New<TeamId>(timeProvider), club.Id, validName.Value, section));
    }

    public bool IsOwnedBy(OrganisationId organisationId) => OrganisationId == organisationId;

    public Result Rename(string? name)
    {
        var validName = ValidateName(name);
        if (validName.IsFailure)
        {
            return validName;
        }

        Name = validName.Value;

        return Result.Success();
    }

    public void ChangeSection(Section section) => Section = section;

    /// <summary>
    /// Adds a gymnast of the same club. A gymnast of another club is reported as not found, because the team's
    /// club may not see it (ADR-0006).
    /// </summary>
    public Result AddGymnast(Gymnast gymnast)
    {
        ArgumentNullException.ThrowIfNull(gymnast);

        if (!gymnast.IsOwnedBy(OrganisationId))
        {
            return Result.Failure(GymnastErrors.NotFound);
        }

        return _gymnastIds.Add(gymnast.Id) ? Result.Success() : Result.Failure(TeamErrors.GymnastAlreadyInTeam);
    }

    public Result RemoveGymnast(GymnastId gymnastId) =>
        _gymnastIds.Remove(gymnastId) ? Result.Success() : Result.Failure(TeamErrors.GymnastNotInTeam);

    private static Result<string> ValidateName(string? name)
    {
        var trimmed = name?.Trim() ?? string.Empty;

        if (trimmed.Length == 0)
        {
            return Result.Failure<string>(TeamErrors.NameRequired);
        }

        return trimmed.Length > NameMaxLength ? Result.Failure<string>(TeamErrors.NameTooLong) : Result.Success(trimmed);
    }
}
