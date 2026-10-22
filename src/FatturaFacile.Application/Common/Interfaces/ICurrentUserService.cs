// <copyright file="ICurrentUserService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Exposes context and identity information about the currently authenticated operator executing application requests.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the unique identifier of the authenticated user, or <see langword="null"/> if the operation executes unauthenticated.
    /// </summary>
    public Guid? UserId { get; }

    /// <summary>
    /// Gets the login name or username of the authenticated user, or <see langword="null"/> when anonymous.
    /// </summary>
    public string? Username { get; }

    /// <summary>
    /// Gets the full display name of the authenticated user, or <see langword="null"/> when unavailable.
    /// </summary>
    public string? FullName { get; }

    /// <summary>
    /// Gets the security role string associated with the user session, or <see langword="null"/> if unassigned.
    /// </summary>
    public string? Role { get; }

    /// <summary>
    /// Gets a value indicating whether a valid user security principal is actively authenticated.
    /// </summary>
    public bool IsAuthenticated { get; }

    /// <summary>
    /// Binds the authenticated user identity details to the current operational context.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="username">The account login name or alias.</param>
    /// <param name="fullName">The full descriptive person or contact name.</param>
    /// <param name="role">The assigned role designation.</param>
    public void SetUser(
        Guid userId,
        string username,
        string fullName,
        string role);

    /// <summary>
    /// Clears any established user session identity, resetting context to an anonymous unauthenticated state.
    /// </summary>
    public void Clear();
}
