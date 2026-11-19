namespace TeamGym.Arena.Common.Events;

/// <summary>Something that happened inside one module's domain model. It never leaves the module.</summary>
public interface IDomainEvent
{
    /// <summary>When the event happened, in UTC, taken from <see cref="TimeProvider"/>.</summary>
    DateTimeOffset OccurredAt { get; }
}
