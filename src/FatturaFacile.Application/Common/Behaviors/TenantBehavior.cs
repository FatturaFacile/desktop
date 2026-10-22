// <copyright file="TenantBehavior.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

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
public sealed partial class TenantBehavior<TRequest, TResponse>(
    ITenantProvider tenantProvider,
    ILogger<TenantBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ITenantProvider tenantProvider = tenantProvider;

    private readonly ILogger<TenantBehavior<TRequest, TResponse>> logger = logger;

    /// <summary>
    /// Handle.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="next">The next.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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
