namespace TeamGym.Arena.Common.Results;

/// <summary>The kind of an <see cref="Error"/>. The API host maps it to an HTTP status code.</summary>
public enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Conflict,
    Forbidden,
}
