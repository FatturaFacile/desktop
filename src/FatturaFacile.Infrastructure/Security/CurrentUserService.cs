// <copyright file="CurrentUserService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;

namespace FatturaFacile.Infrastructure.Security;

/// <summary>
/// Maintains and exposes the authenticated user context for the current desktop application session.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{

    /// <summary>
    /// Gets the unique identifier of the authenticated user, or <see langword="null"/> when unauthenticated.
    /// </summary>
    public Guid? UserId { get; private set; }

    /// <summary>
    /// Gets the username of the authenticated user, or <see langword="null"/> when unauthenticated.
    /// </summary>
    public string? Username { get; private set; }

    /// <summary>
    /// Gets the full display name of the authenticated user, or <see langword="null"/> when unauthenticated.
    /// </summary>
    public string? FullName { get; private set; }

    /// <summary>
    /// Gets the security role string (e.g., Admin, SuperAdmin, Accountant) of the authenticated user.
    /// </summary>
    public string? Role { get; private set; }

    /// <summary>
    /// Gets a value indicating whether an active user session is currently authenticated.
    /// </summary>
    public bool IsAuthenticated => UserId.HasValue;

    /// <summary>
    /// Sets the active user credentials and role for the current runtime session.
    /// </summary>
    /// <param name="userId">The unique identifier of the authenticated user.</param>
    /// <param name="username">The login username.</param>
    /// <param name="fullName">The full display name.</param>
    /// <param name="role">The assigned role name.</param>
    public void SetUser(
        Guid   userId,
        string username,
        string fullName,
        string role)
    {
        UserId   = userId;
        Username = username;
        FullName = fullName;
        Role     = role;
    }

    /// <summary>
    /// Clears the active user session state, reverting to an unauthenticated status.
    /// </summary>
    public void Clear()
    {
        UserId   = null;
        Username = null;
        FullName = null;
        Role     = null;
    }

}
