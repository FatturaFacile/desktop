// <copyright file="ICurrentUserService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Defines the contract for current user service.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the user id.
    /// </summary>
    public Guid? UserId { get; }

    /// <summary>
    /// Gets the username.
    /// </summary>
    public string? Username { get; }

    /// <summary>
    /// Gets the full name.
    /// </summary>
    public string? FullName { get; }

    /// <summary>
    /// Gets the role.
    /// </summary>
    public string? Role { get; }

    /// <summary>
    /// Gets a value indicating whether authenticated.
    /// </summary>
    public bool IsAuthenticated { get; }

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
        string role);

    /// <summary>
    /// Clear.
    /// </summary>
    public void Clear();
}
