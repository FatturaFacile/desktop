// <copyright file="CurrentUserService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;

namespace FatturaFacile.Infrastructure.Security;

/// <summary>
/// Represents current user service.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    /// <summary>
    /// Gets the user id.
    /// </summary>
    public Guid? UserId { get; private set; }

    /// <summary>
    /// Gets the username.
    /// </summary>
    public string? Username { get; private set; }

    /// <summary>
    /// Gets the full name.
    /// </summary>
    public string? FullName { get; private set; }

    /// <summary>
    /// Gets the role.
    /// </summary>
    public string? Role { get; private set; }

    /// <summary>
    /// Gets a value indicating whether authenticated.
    /// </summary>
    public bool IsAuthenticated => UserId.HasValue;

    /// <summary>
    /// Set User.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <param name="username">The username.</param>
    /// <param name="fullName">The full name.</param>
    /// <param name="role">The role.</param>
    public void SetUser(
        Guid userId,
        string username,
        string fullName,
        string role)
    {
        UserId = userId;
        Username = username;
        FullName = fullName;
        Role = role;
    }

    /// <summary>
    /// Clear.
    /// </summary>
    public void Clear()
    {
        UserId = null;
        Username = null;
        FullName = null;
        Role = null;
    }
}
