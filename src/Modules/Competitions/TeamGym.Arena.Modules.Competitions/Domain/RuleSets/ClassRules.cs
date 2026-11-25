using TeamGym.Arena.Common.Results;

namespace TeamGym.Arena.Modules.Competitions.Domain.RuleSets;

/// <summary>
/// A <b>Class</b> that a rule set offers, such as Junior, Senior or a national level, with the team size rules
/// that apply in it (ADR-0006).
/// </summary>
internal sealed class ClassRules
{
    public const int CodeMaxLength = 32;

    public const int NameMaxLength = 100;

    private ClassRules(string code, string name, TeamComposition composition)
    {
        Code = code;
        Name = name;
        Composition = composition;
    }

    /// <summary>
    /// A stable code in upper case, such as <c>SENIOR</c>. It is the class's key within a rule set, and the web app
    /// translates it for display.
    /// </summary>
    public string Code { get; }

    /// <summary>The English name, such as <c>Senior</c>.</summary>
    public string Name { get; }

    public TeamComposition Composition { get; }

    /// <summary>
    /// Creates a class. The code is trimmed and put in upper case, and may hold only ASCII letters, digits and
    /// hyphens.
    /// </summary>
    public static Result<ClassRules> Create(string? code, string? name, TeamComposition composition)
    {
        ArgumentNullException.ThrowIfNull(composition);

        var normalisedCode = NormaliseCode(code);
        if (normalisedCode.Length == 0
            || normalisedCode.Length > CodeMaxLength
            || !normalisedCode.All(c => char.IsAsciiLetterOrDigit(c) || c == '-'))
        {
            return Result.Failure<ClassRules>(RuleSetErrors.InvalidClassCode);
        }

        var trimmedName = name?.Trim() ?? string.Empty;
        if (trimmedName.Length == 0 || trimmedName.Length > NameMaxLength)
        {
            return Result.Failure<ClassRules>(RuleSetErrors.InvalidClassName);
        }

        return Result.Success(new ClassRules(normalisedCode, trimmedName, composition));
    }

    /// <summary>Puts a class code in the form that <see cref="Code"/> stores, so look-ups ignore case.</summary>
    public static string NormaliseCode(string? code) => code?.Trim().ToUpperInvariant() ?? string.Empty;

    public ClassRules WithComposition(TeamComposition composition)
    {
        ArgumentNullException.ThrowIfNull(composition);

        return new ClassRules(Code, Name, composition);
    }
}
