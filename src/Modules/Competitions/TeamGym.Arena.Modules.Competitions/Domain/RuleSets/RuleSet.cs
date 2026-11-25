using TeamGym.Arena.Common.Results;
using TeamGym.Arena.Modules.Competitions.Contracts;

namespace TeamGym.Arena.Modules.Competitions.Domain.RuleSets;

/// <summary>
/// A named, versioned set of TeamGym rules that an organiser picks a competition's rules from (ADR-0006). The
/// platform ships built-in presets, see <see cref="RuleSetPresets"/>. A competition copies the rules it uses, so a
/// new version of a preset never changes a competition that already exists.
/// </summary>
internal sealed class RuleSet
{
    public const int CodeMaxLength = 32;

    public const int NameMaxLength = 200;

    private RuleSet(RuleSetId id, string code, string name, int version, TeamGymRules rules)
    {
        Id = id;
        Code = code;
        Name = name;
        Version = version;
        Rules = rules;
    }

    public RuleSetId Id { get; }

    /// <summary>A stable code in upper case, such as <c>EG-TEAMGYM</c>.</summary>
    public string Code { get; }

    public string Name { get; }

    /// <summary>The version of the rules, starting at 1.</summary>
    public int Version { get; }

    public TeamGymRules Rules { get; }

    /// <summary>Creates a rule set with a fixed, well-known ID, as the built-in presets have.</summary>
    public static Result<RuleSet> Create(RuleSetId id, string? code, string? name, int version, TeamGymRules rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        var normalisedCode = code?.Trim().ToUpperInvariant() ?? string.Empty;
        if (normalisedCode.Length == 0 || normalisedCode.Length > CodeMaxLength)
        {
            return Result.Failure<RuleSet>(RuleSetErrors.InvalidCode);
        }

        var trimmedName = name?.Trim() ?? string.Empty;
        if (trimmedName.Length == 0 || trimmedName.Length > NameMaxLength)
        {
            return Result.Failure<RuleSet>(RuleSetErrors.InvalidName);
        }

        if (version < 1)
        {
            return Result.Failure<RuleSet>(RuleSetErrors.InvalidVersion);
        }

        return Result.Success(new RuleSet(id, normalisedCode, trimmedName, version, rules));
    }
}
