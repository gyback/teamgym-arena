namespace TeamGym.Arena.Common.Events;

/// <summary>Publishes integration events to the handlers in other modules, in process (ADR-0004).</summary>
public interface IIntegrationEventPublisher
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken)
        where TEvent : IIntegrationEvent;
}
