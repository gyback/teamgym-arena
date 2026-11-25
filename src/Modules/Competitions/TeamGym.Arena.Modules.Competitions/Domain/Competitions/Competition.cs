using TeamGym.Arena.Common;
using TeamGym.Arena.Common.Identifiers;
using TeamGym.Arena.Common.Results;
using TeamGym.Arena.Modules.Competitions.Contracts;
using TeamGym.Arena.Modules.Competitions.Domain.RuleSets;
using TeamGym.Arena.Modules.Organisations.Contracts;

namespace TeamGym.Arena.Modules.Competitions.Domain.Competitions;

/// <summary>
/// A competition, hosted by one organisation of any type (ADR-0006). A draft is private to its organiser; once
/// published, every organisation may see it. The competition holds its own copy of the rules it was set up from,
/// which the organiser may narrow and tailor while it is a draft, and which is fixed once it is published.
/// </summary>
internal sealed class Competition
{
    public const int NameMaxLength = 200;

    private Competition(
        CompetitionId id,
        OrganisationId organiserOrganisationId,
        string name,
        RuleSet ruleSet)
    {
        Id = id;
        OrganiserOrganisationId = organiserOrganisationId;
        Name = name;
        RuleSetId = ruleSet.Id;
        RuleSetVersion = ruleSet.Version;
        Rules = ruleSet.Rules;
        Status = CompetitionStatus.Draft;
    }

    public CompetitionId Id { get; }

    /// <summary>The organisation that hosts the competition: a club, a region or a federation.</summary>
    public OrganisationId OrganiserOrganisationId { get; }

    public string Name { get; private set; }

    /// <summary>The rule set that <see cref="Rules"/> was copied from.</summary>
    public RuleSetId RuleSetId { get; private set; }

    /// <summary>The version of the rule set that <see cref="Rules"/> was copied from.</summary>
    public int RuleSetVersion { get; private set; }

    /// <summary>The competition's own copy of the rules, which changes to the rule set never alter.</summary>
    public TeamGymRules Rules { get; private set; }

    public CompetitionStatus Status { get; private set; }

    public bool IsDraft => Status == CompetitionStatus.Draft;

    /// <summary>Creates a draft competition that offers everything <paramref name="ruleSet"/> offers.</summary>
    public static Result<Competition> Create(
        OrganisationId organiserOrganisationId,
        string? name,
        RuleSet ruleSet,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(ruleSet);
        ArgumentNullException.ThrowIfNull(timeProvider);

        var validName = ValidateName(name);
        if (validName.IsFailure)
        {
            return Result.Failure<Competition>(validName.Error);
        }

        return Result.Success(new Competition(
            StronglyTypedId.New<CompetitionId>(timeProvider),
            organiserOrganisationId,
            validName.Value,
            ruleSet));
    }

    public bool IsOrganisedBy(OrganisationId organisationId) => OrganiserOrganisationId == organisationId;

    /// <summary>
    /// Whether <paramref name="organisationId"/> may see the competition: its organiser always, and every
    /// organisation once it is published (ADR-0006).
    /// </summary>
    public bool IsVisibleTo(OrganisationId organisationId) => !IsDraft || IsOrganisedBy(organisationId);

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

    /// <summary>
    /// Copies the rules of <paramref name="ruleSet"/>, narrowed to the given sections, classes and disciplines.
    /// This replaces the current rules, including any tailored class, so it may also switch to another rule set.
    /// </summary>
    public Result SelectRules(
        RuleSet ruleSet,
        IReadOnlyCollection<Section> sections,
        IReadOnlyCollection<string> classCodes,
        IReadOnlyCollection<Discipline> disciplines)
    {
        ArgumentNullException.ThrowIfNull(ruleSet);

        if (!IsDraft)
        {
            return Result.Failure(CompetitionErrors.RulesFixed);
        }

        var rules = ruleSet.Rules.Restrict(sections, classCodes, disciplines);
        if (rules.IsFailure)
        {
            return rules;
        }

        RuleSetId = ruleSet.Id;
        RuleSetVersion = ruleSet.Version;
        Rules = rules.Value;

        return Result.Success();
    }

    /// <summary>Tailors the team size rules of one class for this competition only.</summary>
    public Result TailorClassComposition(string? classCode, TeamComposition composition)
    {
        ArgumentNullException.ThrowIfNull(composition);

        if (!IsDraft)
        {
            return Result.Failure(CompetitionErrors.RulesFixed);
        }

        var rules = Rules.WithClassComposition(classCode, composition);
        if (rules.IsFailure)
        {
            return rules;
        }

        Rules = rules.Value;

        return Result.Success();
    }

    /// <summary>Publishes the competition, which makes it visible to every organisation and fixes its rules.</summary>
    public Result Publish()
    {
        if (!IsDraft)
        {
            return Result.Failure(CompetitionErrors.AlreadyPublished);
        }

        Status = CompetitionStatus.Published;

        return Result.Success();
    }

    /// <summary>Checks a team against the competition's own rules.</summary>
    public Result ValidateTeam(Section section, string? classCode, TeamRoster roster) =>
        Rules.ValidateTeam(section, classCode, roster);

    public CompetitionSummary ToSummary() => new(Id, OrganiserOrganisationId, Name, IsPublished: !IsDraft);

    private static Result<string> ValidateName(string? name)
    {
        var trimmed = name?.Trim() ?? string.Empty;

        if (trimmed.Length == 0)
        {
            return Result.Failure<string>(CompetitionErrors.NameRequired);
        }

        return trimmed.Length > NameMaxLength
            ? Result.Failure<string>(CompetitionErrors.NameTooLong)
            : Result.Success(trimmed);
    }
}
