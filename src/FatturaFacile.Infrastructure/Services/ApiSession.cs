// <copyright file="ApiSession.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Security.Iam;

namespace FatturaFacile.Infrastructure.Services;

/// <summary>
/// The API session of the desktop client. Ending it purges the bearer token and every cached right of the user, so after a
/// remote revocation nothing works offline either.
/// </summary>
/// <param name="authorization">The authorization service holding the snapshot in force.</param>
/// <param name="cache">The local permission cache.</param>
/// <param name="currentUser">The signed-in user.</param>
public sealed class ApiSession(IAuthorizationService authorization, IPermissionCache cache, ICurrentUserService currentUser) : IApiSession
{
    private volatile string? _token;

    /// <inheritdoc/>
    public event EventHandler<ApiProblemEventArgs>? ProblemRaised;

    /// <inheritdoc/>
    public string? BearerToken => _token;

    /// <inheritdoc/>
    public void Start(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        _token = token;
    }

    /// <inheritdoc/>
    public async Task EndAsync(ApiProblemKind? reason, CancellationToken cancellationToken = default)
    {
        _token = null;
        authorization.Apply(snapshot: null);

        if (currentUser.User is { } user)
        {
            await cache.RevokeUserAsync(user.UserId, cancellationToken).ConfigureAwait(false);
        }

        if (reason is { } kind)
        {
            ProblemRaised?.Invoke(this, new ApiProblemEventArgs(kind));
        }
    }

    /// <inheritdoc/>
    public void Report(ApiProblemKind kind) => ProblemRaised?.Invoke(this, new ApiProblemEventArgs(kind));
}
