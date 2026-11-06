// <copyright file="ICurrentUserService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Holds the signed-in user of the desktop session and answers authorization questions.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the signed-in user, or null.
    /// </summary>
    public AuthenticatedUser? User { get; }

    /// <summary>
    /// Gets a value indicating whether a user is signed in.
    /// </summary>
    public bool IsAuthenticated { get; }

    /// <summary>
    /// Signs a user in for the session.
    /// </summary>
    /// <param name="user">The authenticated user.</param>
    public void SignIn(AuthenticatedUser user);

    /// <summary>
    /// Signs the current user out.
    /// </summary>
    public void SignOut();

    /// <summary>
    /// Determines whether the signed-in user can access the tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <returns><see langword="true"/> when accessible.</returns>
    public bool CanAccessTenant(Guid tenantId);

    /// <summary>
    /// Determines whether the signed-in user holds a permission in the tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="permission">The permission.</param>
    /// <returns><see langword="true"/> when granted.</returns>
    public bool HasPermission(Guid tenantId, Permission permission);
}
