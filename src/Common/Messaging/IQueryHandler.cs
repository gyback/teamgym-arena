using TeamGym.Arena.Common.Results;

namespace TeamGym.Arena.Common.Messaging;

/// <summary>
/// Handles a query in a feature slice. Endpoints inject the handler directly; there is no mediator (ADR-0004).
/// </summary>
public interface IQueryHandler<in TQuery, TResponse>
{
    Task<Result<TResponse>> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
