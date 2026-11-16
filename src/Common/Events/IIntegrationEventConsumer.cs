namespace TeamGym.Arena.Common.Events;

/// <summary>Reacts to an integration event published by another module.</summary>
public interface IIntegrationEventConsumer<in TEvent>
    where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent integrationEvent, CancellationToken cancellationToken);
}
