namespace TeamGym.Arena.Common.Identifiers;

/// <summary>Creates new strongly typed IDs.</summary>
public static class StronglyTypedId
{
    /// <summary>
    /// Creates a new ID from a UUIDv7 stamped with the current time of <paramref name="timeProvider"/>.
    /// </summary>
    /// <remarks>
    /// IDs sort by creation time to the millisecond only. The remaining bits are random, so IDs created within
    /// the same millisecond (or with a test clock that is not advanced) are neither deterministic nor ordered.
    /// Do not rely on ID order for anything finer than a millisecond.
    /// </remarks>
    public static TId New<TId>(TimeProvider timeProvider)
        where TId : struct, IStronglyTypedId<TId>
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        return TId.From(Guid.CreateVersion7(timeProvider.GetUtcNow()));
    }
}
