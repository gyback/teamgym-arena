namespace TeamGym.Arena.Common.Events;

/// <summary>
/// A fact that one module publishes for other modules to react to (ADR-0004). Declare it in the publishing
/// module's <c>.Contracts</c> project. Carry identifiers only, never personal data.
/// </summary>
public interface IIntegrationEvent
{
    Guid EventId { get; }

    /// <summary>When the event happened, in UTC, taken from <see cref="TimeProvider"/>.</summary>
    DateTimeOffset OccurredAt { get; }
}
