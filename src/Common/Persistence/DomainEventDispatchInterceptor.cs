using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using TeamGym.Arena.Common.Events;

namespace TeamGym.Arena.Common.Persistence;

/// <summary>
/// Dispatches the domain events of tracked <see cref="IHasDomainEvents"/> aggregates to the module's
/// <see cref="IDomainEventConsumer{TEvent}"/>s before the changes are saved, so that the consumers' own changes are
/// saved in the same transaction (ADR-0007). Each event goes to the consumers registered for its exact type.
/// </summary>
/// <remarks>
/// A consumer may raise further events. They are dispatched in further rounds until none are left, up to
/// <see cref="MaxDispatchRounds"/> rounds, so that consumers that keep raising events cannot loop forever.
/// Domain events need <see cref="DbContext.SaveChangesAsync(CancellationToken)"/>: the synchronous
/// <see cref="DbContext.SaveChanges()"/> throws when there are events to dispatch.
/// </remarks>
public sealed class DomainEventDispatchInterceptor(IServiceProvider services) : SaveChangesInterceptor
{
    /// <summary>The number of dispatch rounds after which the interceptor gives up.</summary>
    public const int MaxDispatchRounds = 10;

    private static readonly MethodInfo s_dispatchMethod = typeof(DomainEventDispatchInterceptor)
        .GetMethod(nameof(DispatchToConsumersAsync), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly ConcurrentDictionary<Type, Func<IServiceProvider, IDomainEvent, CancellationToken, Task>> s_dispatchers = new();

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        if (eventData.Context is { } context && AggregatesWithEvents(context).Count > 0)
        {
            throw new InvalidOperationException(
                "Aggregates with domain events must be saved with SaveChangesAsync, which dispatches the events.");
        }

        return result;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        if (eventData.Context is { } context)
        {
            await DispatchAsync(context, cancellationToken).ConfigureAwait(false);
        }

        return result;
    }

    private async Task DispatchAsync(DbContext context, CancellationToken cancellationToken)
    {
        for (var round = 0; round < MaxDispatchRounds; round++)
        {
            var aggregates = AggregatesWithEvents(context);
            if (aggregates.Count == 0)
            {
                return;
            }

            // Take every event first, so that events that the consumers raise belong to the next round.
            var events = aggregates.SelectMany(aggregate => aggregate.DomainEvents).ToList();
            foreach (var aggregate in aggregates)
            {
                aggregate.ClearDomainEvents();
            }

            foreach (var domainEvent in events)
            {
                var dispatch = s_dispatchers.GetOrAdd(domainEvent.GetType(), CreateDispatcher);
                await dispatch(services, domainEvent, cancellationToken).ConfigureAwait(false);
            }

            // The next round's Entries<T>() detects the changes that the consumers made, including new aggregates.
        }

        if (AggregatesWithEvents(context).Count > 0)
        {
            throw new InvalidOperationException(
                $"Domain event consumers still raised events after {MaxDispatchRounds} rounds. Check them for a cycle.");
        }
    }

    private static List<IHasDomainEvents> AggregatesWithEvents(DbContext context) =>
    [
        .. context.ChangeTracker.Entries<IHasDomainEvents>()
            .Select(entry => entry.Entity)
            .Where(aggregate => aggregate.DomainEvents.Count > 0),
    ];

    private static Func<IServiceProvider, IDomainEvent, CancellationToken, Task> CreateDispatcher(Type eventType) =>
        s_dispatchMethod.MakeGenericMethod(eventType)
            .CreateDelegate<Func<IServiceProvider, IDomainEvent, CancellationToken, Task>>();

    private static async Task DispatchToConsumersAsync<TEvent>(
        IServiceProvider services,
        IDomainEvent domainEvent,
        CancellationToken cancellationToken)
        where TEvent : IDomainEvent
    {
        foreach (var consumer in services.GetServices<IDomainEventConsumer<TEvent>>())
        {
            await consumer.HandleAsync((TEvent)domainEvent, cancellationToken).ConfigureAwait(false);
        }
    }
}
