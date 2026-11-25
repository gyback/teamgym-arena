namespace TeamGym.Arena.Common.Events;

/// <summary>
/// Reacts to a domain event of its own module, inside the same unit of work: the consumer runs before the changes are
/// saved, so whatever it changes in the module's <c>DbContext</c> is saved in the same transaction (ADR-0007).
/// Register consumers in the module's registration. Use an <see cref="IIntegrationEvent"/> to reach other modules.
/// </summary>
public interface IDomainEventConsumer<in TEvent>
    where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken);
}
