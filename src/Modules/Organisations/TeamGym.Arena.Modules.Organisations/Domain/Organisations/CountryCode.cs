using TeamGym.Arena.Common.Results;

namespace TeamGym.Arena.Modules.Organisations.Domain.Organisations;

/// <summary>
/// An ISO 3166-1 alpha-2 country code in upper case, such as <c>SE</c>. The exceptionally reserved code <c>EU</c>
/// is accepted for European organisations. Only the format is checked, not that the code is assigned.
/// </summary>
internal readonly record struct CountryCode
{
    private CountryCode(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<CountryCode> Create(string? value)
    {
        var trimmed = value?.Trim() ?? string.Empty;

        if (trimmed.Length != 2 || !trimmed.All(char.IsAsciiLetter))
        {
            return Result.Failure<CountryCode>(OrganisationErrors.InvalidCountryCode);
        }

        return Result.Success(new CountryCode(trimmed.ToUpperInvariant()));
    }

    public override string ToString() => Value;
}
