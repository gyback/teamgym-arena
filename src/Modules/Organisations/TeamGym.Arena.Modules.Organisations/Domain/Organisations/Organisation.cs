using TeamGym.Arena.Common.Identifiers;
using TeamGym.Arena.Common.Results;
using TeamGym.Arena.Modules.Competitions.Contracts;
using TeamGym.Arena.Modules.Organisations.Contracts;
using TeamGym.Arena.Modules.Organisations.Domain.Teams;
using TeamGym.Arena.Modules.Users.Contracts;

namespace TeamGym.Arena.Modules.Organisations.Domain.Organisations;

/// <summary>
/// A club, region or federation (ADR-0006). All three share this aggregate so that every reference to an
/// organisation is an <see cref="OrganisationId"/>, and a hierarchy can be built with
/// <see cref="ParentOrganisationId"/> without changing the schema. The aggregate also owns its memberships.
/// </summary>
internal sealed class Organisation
{
    public const int NameMaxLength = 200;

    public const int ShortNameMaxLength = 20;

    private readonly List<OrganisationMembership> _memberships = [];

    private Organisation(
        OrganisationId id,
        string name,
        string? shortName,
        OrganisationType type,
        CountryCode countryCode,
        OrganisationId? parentOrganisationId)
    {
        Id = id;
        Name = name;
        ShortName = shortName;
        Type = type;
        CountryCode = countryCode;
        ParentOrganisationId = parentOrganisationId;
        Status = OrganisationStatus.Active;
    }

    public OrganisationId Id { get; }

    public string Name { get; private set; }

    /// <summary>An optional short name or code, such as <c>GFF</c> or <c>SWE</c>.</summary>
    public string? ShortName { get; private set; }

    public OrganisationType Type { get; }

    public CountryCode CountryCode { get; }

    public OrganisationId? ParentOrganisationId { get; private set; }

    /// <summary>The rule set that competitions hosted by this organisation start from, if any (ADR-0006).</summary>
    public RuleSetId? DefaultRuleSetId { get; private set; }

    public OrganisationStatus Status { get; private set; }

    public bool IsActive => Status == OrganisationStatus.Active;

    public IReadOnlyCollection<OrganisationMembership> Memberships => _memberships;

    /// <summary>Creates an active organisation, optionally as a member of <paramref name="parent"/>.</summary>
    public static Result<Organisation> Create(
        string? name,
        string? shortName,
        OrganisationType type,
        string? countryCode,
        Organisation? parent,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        var names = ValidateNames(name, shortName);
        if (names.IsFailure)
        {
            return Result.Failure<Organisation>(names.Error);
        }

        var country = CountryCode.Create(countryCode);
        if (country.IsFailure)
        {
            return Result.Failure<Organisation>(country.Error);
        }

        if (parent is not null)
        {
            var parentCheck = ValidateParent(parent, type);
            if (parentCheck.IsFailure)
            {
                return Result.Failure<Organisation>(parentCheck.Error);
            }
        }

        return Result.Success(new Organisation(
            StronglyTypedId.New<OrganisationId>(timeProvider),
            names.Value.Name,
            names.Value.ShortName,
            type,
            country.Value,
            parent?.Id));
    }

    public Result Rename(string? name, string? shortName)
    {
        if (!IsActive)
        {
            return Result.Failure(OrganisationErrors.Archived);
        }

        var names = ValidateNames(name, shortName);
        if (names.IsFailure)
        {
            return names;
        }

        (Name, ShortName) = names.Value;

        return Result.Success();
    }

    /// <summary>
    /// Moves the organisation under <paramref name="parent"/>, or to the top of the hierarchy when it is
    /// <see langword="null"/>.
    /// </summary>
    /// <param name="parent">The new parent, or <see langword="null"/> for none.</param>
    /// <param name="ancestorsOfParent">
    /// The IDs of every organisation above <paramref name="parent"/>, which the caller loads so that a cycle can be
    /// detected.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="parent"/> has a parent of its own that <paramref name="ancestorsOfParent"/> does not contain.
    /// </exception>
    public Result ChangeParent(Organisation? parent, IReadOnlyCollection<OrganisationId> ancestorsOfParent)
    {
        ArgumentNullException.ThrowIfNull(ancestorsOfParent);

        if (!IsActive)
        {
            return Result.Failure(OrganisationErrors.Archived);
        }

        if (parent is not null)
        {
            if (parent.ParentOrganisationId is { } grandparentId && !ancestorsOfParent.Contains(grandparentId))
            {
                throw new ArgumentException(
                    "The ancestors of the parent must include the parent's own parent, or a cycle could go undetected.",
                    nameof(ancestorsOfParent));
            }

            if (parent.Id == Id || ancestorsOfParent.Contains(Id))
            {
                return Result.Failure(OrganisationErrors.HierarchyCycle);
            }

            var parentCheck = ValidateParent(parent, Type);
            if (parentCheck.IsFailure)
            {
                return parentCheck;
            }
        }

        ParentOrganisationId = parent?.Id;

        return Result.Success();
    }

    public Result SetDefaultRuleSet(RuleSetId? ruleSetId)
    {
        if (!IsActive)
        {
            return Result.Failure(OrganisationErrors.Archived);
        }

        DefaultRuleSetId = ruleSetId;

        return Result.Success();
    }

    public Result Archive()
    {
        if (!IsActive)
        {
            return Result.Failure(OrganisationErrors.Archived);
        }

        Status = OrganisationStatus.Archived;

        return Result.Success();
    }

    /// <summary>
    /// Gives <paramref name="userId"/> a <paramref name="role"/> in this organisation. The same user may hold
    /// several roles here and memberships in any number of other organisations.
    /// </summary>
    public Result<OrganisationMembership> AddMembership(UserId userId, OrganisationRole role, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        if (!IsActive)
        {
            return Result.Failure<OrganisationMembership>(OrganisationErrors.Archived);
        }

        if (_memberships.Exists(m => m.UserId == userId && m.Role == role))
        {
            return Result.Failure<OrganisationMembership>(OrganisationErrors.DuplicateMembership);
        }

        var membership = new OrganisationMembership(
            StronglyTypedId.New<OrganisationMembershipId>(timeProvider),
            Id,
            userId,
            role,
            timeProvider.GetUtcNow());

        _memberships.Add(membership);

        return Result.Success(membership);
    }

    public Result RemoveMembership(OrganisationMembershipId membershipId)
    {
        var removed = _memberships.RemoveAll(m => m.Id == membershipId);

        return removed == 0 ? Result.Failure(OrganisationErrors.MembershipNotFound) : Result.Success();
    }

    /// <summary>
    /// Assigns a coach to one of this organisation's teams. A team owned by another organisation is reported as
    /// not found, because this organisation may not see it (ADR-0006).
    /// </summary>
    public Result AssignCoachToTeam(OrganisationMembershipId membershipId, Team team)
    {
        ArgumentNullException.ThrowIfNull(team);

        if (!IsActive)
        {
            return Result.Failure(OrganisationErrors.Archived);
        }

        var membership = FindMembership(membershipId);
        if (membership is null)
        {
            return Result.Failure(OrganisationErrors.MembershipNotFound);
        }

        if (!team.IsOwnedBy(Id))
        {
            return Result.Failure(TeamErrors.NotFound);
        }

        if (membership.Role != OrganisationRole.Coach)
        {
            return Result.Failure(OrganisationErrors.OnlyCoachesHaveTeams);
        }

        return membership.AssignTeam(team.Id)
            ? Result.Success()
            : Result.Failure(OrganisationErrors.CoachAlreadyAssignedToTeam);
    }

    public Result UnassignCoachFromTeam(OrganisationMembershipId membershipId, TeamId teamId)
    {
        var membership = FindMembership(membershipId);
        if (membership is null)
        {
            return Result.Failure(OrganisationErrors.MembershipNotFound);
        }

        return membership.UnassignTeam(teamId)
            ? Result.Success()
            : Result.Failure(OrganisationErrors.CoachNotAssignedToTeam);
    }

    /// <summary>Checks that this organisation can own gymnasts and teams: it must be an active club.</summary>
    public Result EnsureCanOwnClubResources()
    {
        if (!IsActive)
        {
            return Result.Failure(OrganisationErrors.Archived);
        }

        return Type == OrganisationType.Club ? Result.Success() : Result.Failure(OrganisationErrors.NotAClub);
    }

    private OrganisationMembership? FindMembership(OrganisationMembershipId membershipId) =>
        _memberships.Find(m => m.Id == membershipId);

    private static Result ValidateParent(Organisation parent, OrganisationType childType)
    {
        if (!parent.IsActive)
        {
            return Result.Failure(OrganisationErrors.ParentArchived);
        }

        return OrganisationHierarchy.CanBeParentOf(parent.Type, childType)
            ? Result.Success()
            : Result.Failure(OrganisationErrors.InvalidParentType);
    }

    private static Result<(string Name, string? ShortName)> ValidateNames(string? name, string? shortName)
    {
        var trimmedName = name?.Trim() ?? string.Empty;
        if (trimmedName.Length == 0)
        {
            return Result.Failure<(string, string?)>(OrganisationErrors.NameRequired);
        }

        if (trimmedName.Length > NameMaxLength)
        {
            return Result.Failure<(string, string?)>(OrganisationErrors.NameTooLong);
        }

        var trimmedShortName = string.IsNullOrWhiteSpace(shortName) ? null : shortName.Trim();
        if (trimmedShortName?.Length > ShortNameMaxLength)
        {
            return Result.Failure<(string, string?)>(OrganisationErrors.ShortNameTooLong);
        }

        return Result.Success<(string, string?)>((trimmedName, trimmedShortName));
    }
}
