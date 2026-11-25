using TeamGym.Arena.Common.Results;

namespace TeamGym.Arena.Modules.Organisations.Domain.Gymnasts;

/// <summary>Gymnast errors. Like every <see cref="Error"/>, they never contain the gymnast's personal data.</summary>
internal static class GymnastErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Organisations.GymnastNotFound", "The organisation has no such gymnast.");

    public static readonly Error NameRequired = Error.Validation(
        "Organisations.GymnastNameRequired", "A gymnast needs a given name and a family name.");

    public static readonly Error NameTooLong = Error.Validation(
        "Organisations.GymnastNameTooLong", $"A gymnast's given name and family name are each at most {Gymnast.NameMaxLength} characters.");

    public static readonly Error DateOfBirthInFuture = Error.Validation(
        "Organisations.GymnastDateOfBirthInFuture", "A gymnast's date of birth cannot be in the future.");
}
