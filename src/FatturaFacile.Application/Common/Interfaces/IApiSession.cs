// <copyright file="IApiSession.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// The API session of the desktop client: the Sanctum bearer token and what to do when the server withdraws it.
/// </summary>
public interface IApiSession
{
    /// <summary>
    /// Raised when the API refuses the session or a request in one of the ways of <see cref="ApiProblemKind"/>.
    /// </summary>
    public event EventHandler<ApiProblemEventArgs>? ProblemRaised;

    /// <summary>
    /// Gets the bearer token, or null when signed out.
    /// </summary>
    public string? BearerToken { get; }

    /// <summary>
    /// Stores the bearer token after a sign-in.
    /// </summary>
    /// <param name="token">The token.</param>
    public void Start(string token);

    /// <summary>
    /// Purges the token and every cached right, then raises <see cref="ProblemRaised"/> when a reason is given.
    /// </summary>
    /// <param name="reason">Why the session ended, or null for a plain sign-out.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task.</returns>
    public Task EndAsync(ApiProblemKind? reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reports a refusal that does not end the session.
    /// </summary>
    /// <param name="kind">The refusal.</param>
    public void Report(ApiProblemKind kind);
}
