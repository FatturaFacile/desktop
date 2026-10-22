// <copyright file="TenantBehavior.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FatturaFacile.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior that enriches the logging and operational execution context with the current tenant identifier.
/// </summary>
/// <typeparam name="TRequest">The type of the request being handled in the pipeline.</typeparam>
/// <typeparam name="TResponse">The type of the response produced by the request handler.</typeparam>
/// <param name="tenantProvider">The tenant provider used to inspect the active tenant context.</param>
/// <param name="logger">The logger instance used for tenant-scoped diagnostic telemetry.</param>
public sealed partial class TenantBehavior<TRequest, TResponse>(
    ITenantProvider tenantProvider,
    ILogger<TenantBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ITenantProvider tenantProvider = tenantProvider;

    private readonly ILogger<TenantBehavior<TRequest, TResponse>> logger = logger;

    /// <summary>
    /// Processes the incoming request, attaching tenant diagnostics if a tenant scope is established.
    /// </summary>
    /// <param name="request">The incoming request instance.</param>
    /// <param name="next">The delegate representing the remaining actions in the MediatR pipeline.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the pipeline execution and returning <typeparamref name="TResponse"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="next"/> is <see langword="null"/>.</exception>
    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        if (tenantProvider.HasTenant)
        {
            LogProcessingUnderTenant(logger, typeof(TRequest).Name, tenantProvider.CurrentTenantId);
        }

        return next(cancellationToken);
    }

    [LoggerMessage(
        EventId = 1, Level = LogLevel.Debug, Message = "Processing {RequestName} under TenantId: {TenantId}")]
    private static partial void LogProcessingUnderTenant(
        ILogger logger,
        string requestName,
        Guid? tenantId);
}
