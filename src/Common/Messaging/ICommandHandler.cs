using TeamGym.Arena.Common.Results;

namespace TeamGym.Arena.Common.Messaging;

/// <summary>
/// Handles a command in a feature slice. Endpoints inject the handler directly; there is no mediator (ADR-0004).
/// </summary>
public interface ICommandHandler<in TCommand>
{
    Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken);
}

/// <summary>Handles a command that returns a <typeparamref name="TResponse"/> when it succeeds.</summary>
public interface ICommandHandler<in TCommand, TResponse>
{
    Task<Result<TResponse>> HandleAsync(TCommand command, CancellationToken cancellationToken);
}
