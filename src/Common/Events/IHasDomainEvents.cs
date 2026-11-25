namespace TeamGym.Arena.Common.Events;

/// <summary>
/// An aggregate root that raises <see cref="IDomainEvent"/>s. When the module's <c>DbContext</c> saves changes, the
/// shared persistence conventions dispatch the raised events to the module's
/// <see cref="IDomainEventConsumer{TEvent}"/>s and clear them (ADR-0007).
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>The events raised since the aggregate was last saved, in the order they were raised.</summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
