namespace TeamGym.Arena.Common.Identifiers;

/// <summary>Creates new strongly typed IDs.</summary>
public static class StronglyTypedId
{
    /// <summary>
    /// Creates a new ID from a UUIDv7 stamped with the current time of <paramref name="timeProvider"/>, so IDs
    /// sort by creation time and tests stay deterministic.
    /// </summary>
    public static TId New<TId>(TimeProvider timeProvider)
        where TId : struct, IStronglyTypedId<TId>
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        return TId.From(Guid.CreateVersion7(timeProvider.GetUtcNow()));
    }
}
