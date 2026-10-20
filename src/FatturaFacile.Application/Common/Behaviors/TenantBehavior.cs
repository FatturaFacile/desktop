using FatturaFacile.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FatturaFacile.Application.Common.Behaviors;

/// <summary>
/// Represents tenant behavior.
/// </summary>
/// <param name="tenantProvider">The tenant provider.</param>
/// <param name="logger">The logger.</param>
/// <typeparam name="TRequest">The type of the t request.</typeparam>
/// <typeparam name="TResponse">The type of the t response.</typeparam>
public sealed class TenantBehavior<TRequest, TResponse>(ITenantProvider tenantProvider, ILogger<TenantBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ITenantProvider _tenantProvider = tenantProvider;

    private readonly ILogger<TenantBehavior<TRequest, TResponse>> _logger = logger;

/// <summary>
    /// Handle.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="next">The next.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_tenantProvider.HasTenant)
        {
            _logger.LogDebug("Processing {RequestName} under TenantId: {TenantId}", typeof(TRequest).Name, _tenantProvider.CurrentTenantId);
        }

        return next(cancellationToken);
    }
}
