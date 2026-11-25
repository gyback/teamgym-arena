using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TeamGym.Arena.Common.Identifiers;

namespace TeamGym.Arena.Common.Persistence;

/// <summary>Stores a strongly typed ID as the <c>uuid</c> it wraps (ADR-0007).</summary>
public sealed class StronglyTypedIdConverter<TId>()
    : ValueConverter<TId, Guid>(id => id.Value, value => s_create(value))
    where TId : struct, IStronglyTypedId<TId>
{
    // A static abstract member cannot appear in an expression tree, but a delegate to it can.
    private static readonly Func<Guid, TId> s_create = TId.From;
}
