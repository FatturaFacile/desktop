using FatturaFacile.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FatturaFacile.Application.Common.Behaviors;

/// <summary>
/// Represents transaction behavior.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="logger">The logger.</param>
/// <typeparam name="TRequest">The type of the t request.</typeparam>
/// <typeparam name="TResponse">The type of the t response.</typeparam>
public sealed class TransactionBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork, ILogger<TransactionBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger = logger;

/// <summary>
    /// Handle.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="next">The next.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken);

        // If request is a command (modifying state), save changes
        var requestName = typeof(TRequest).Name;
        if (requestName.EndsWith("Command", StringComparison.Ordinal))
        {
            _logger.LogDebug("Auto-committing Unit of Work for command {CommandName}", requestName);
            _ = await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return response;
    }
}
