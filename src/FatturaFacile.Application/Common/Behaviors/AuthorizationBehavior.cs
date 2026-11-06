// <copyright file="AuthorizationBehavior.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Common;
using MediatR;

namespace FatturaFacile.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that enforces <see cref="IAuthorizedRequest.RequiredPermission"/> in the current tenant.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="AuthorizationBehavior{TRequest, TResponse}"/> class.
/// </remarks>
/// <param name="currentUserService">The signed-in user.</param>
/// <param name="currentTenantService">The active tenant.</param>
public sealed class AuthorizationBehavior<TRequest, TResponse>(
    ICurrentUserService currentUserService,
    ICurrentTenantService currentTenantService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <inheritdoc/>
    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var required = (request as IAuthorizedRequest)?.RequiredPermission;
        var denied = required is { } permission && !currentUserService.HasPermission(currentTenantService.TenantId, permission);

        return denied
            ? throw new UnauthorizedAccessException($"Permesso '{required}' non concesso all'utente corrente in questa azienda.")
            : next(cancellationToken);
    }
}
