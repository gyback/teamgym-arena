using System.Diagnostics.CodeAnalysis;

namespace TeamGym.Arena.Common.Results;

/// <summary>
/// The outcome of an operation that can fail in an expected way, without using exceptions for control flow.
/// </summary>
public class Result
{
    private protected Result(Error? error)
    {
        Error = error;
    }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null;

    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => Error is not null;

    /// <summary>The failure, or <see langword="null"/> when the operation succeeded.</summary>
    public Error? Error { get; }

    public static Result Success() => new(error: null);

    public static Result Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new Result(error);
    }

    public static Result<TValue> Success<TValue>(TValue value) => new(value);

    public static Result<TValue> Failure<TValue>(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new Result<TValue>(error);
    }
}

/// <summary>The outcome of an operation that returns a <typeparamref name="TValue"/> when it succeeds.</summary>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue value)
        : base(error: null)
    {
        _value = value;
    }

    internal Result(Error error)
        : base(error)
    {
    }

    /// <summary>The value of a successful result.</summary>
    /// <exception cref="InvalidOperationException">The result is a failure.</exception>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"A failed result has no value. Error: {Error.Code}.");
}
