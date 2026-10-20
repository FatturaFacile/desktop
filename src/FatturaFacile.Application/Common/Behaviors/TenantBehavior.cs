using FatturaFacile.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FatturaFacile.Application.Common.Behaviors;

public sealed class TenantBehavior<TRequest, TResponse>(ITenantProvider tenantProvider, ILogger<TenantBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ITenantProvider _tenantProvider = tenantProvider;
    private readonly ILogger<TenantBehavior<TRequest, TResponse>> _logger = logger;

    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_tenantProvider.HasTenant)
        {
            _logger.LogDebug("Processing {RequestName} under TenantId: {TenantId}", typeof(TRequest).Name, _tenantProvider.CurrentTenantId);
        }

        return next(cancellationToken);
    }
}
