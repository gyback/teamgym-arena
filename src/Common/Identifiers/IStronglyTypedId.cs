namespace TeamGym.Arena.Common.Identifiers;

/// <summary>
/// A strongly typed entity ID backed by a UUIDv7 (ADR-0007). Implement it on a <c>readonly record struct</c>,
/// for example <c>public readonly record struct CompetitionId(Guid Value) : IStronglyTypedId&lt;CompetitionId&gt;</c>.
/// </summary>
public interface IStronglyTypedId<TSelf>
    where TSelf : struct, IStronglyTypedId<TSelf>
{
    Guid Value { get; }

    /// <summary>Wraps an existing value, for example one read from the database or a route.</summary>
    static abstract TSelf From(Guid value);
}
