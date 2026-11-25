using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TeamGym.Arena.Common.Persistence;

/// <summary>Mapping conventions that the modules' entity configurations share (ADR-0007).</summary>
public static class EntityTypeBuilderExtensions
{
    /// <summary>The name of the shadow property that holds an aggregate root's row version.</summary>
    public const string RowVersionProperty = "RowVersion";

    /// <summary>
    /// Uses PostgreSQL's <c>xmin</c> system column as the optimistic concurrency token of an aggregate root. Saving
    /// a stale aggregate throws <see cref="DbUpdateConcurrencyException"/>, which the API maps to <c>409 Conflict</c>.
    /// </summary>
    /// <remarks>
    /// <c>xmin</c> changes only when the aggregate root's own row changes. If a command changes only a child entity
    /// and must still conflict with a concurrent change, it has to change the root as well.
    /// </remarks>
    public static EntityTypeBuilder<TEntity> HasXminConcurrencyToken<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Property<uint>(RowVersionProperty)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .IsRowVersion();

        return builder;
    }
}
