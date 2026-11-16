using System.Diagnostics.CodeAnalysis;

namespace TeamGym.Arena.Common.Results;

/// <summary>
/// An expected domain or application failure. <paramref name="Code"/> is a stable, machine-readable identifier
/// such as <c>Competitions.ApplicationWindowClosed</c>. Never put personal data in an error.
/// </summary>
[SuppressMessage(
    "Naming",
    "CA1716:Identifiers should not match keywords",
    Justification = "ADR-0004 names this type Error. It clashes only with a Visual Basic keyword, and the shared kernel has no Visual Basic consumers.")]
public sealed record Error(string Code, string Description, ErrorType Type)
{
    public static Error Failure(string code, string description) => new(code, description, ErrorType.Failure);

    public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);

    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);

    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);

    public static Error Forbidden(string code, string description) => new(code, description, ErrorType.Forbidden);
}
