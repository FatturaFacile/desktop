// <copyright file="CurrentUserService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Infrastructure.Services;

/// <summary>
/// In-memory session of the signed-in desktop user.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    /// <inheritdoc/>
    public AuthenticatedUser? User { get; private set; }

    /// <inheritdoc/>
    public bool IsAuthenticated => User is not null;

    /// <inheritdoc/>
    public void SignIn(AuthenticatedUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        User = user;
    }

    /// <inheritdoc/>
    public void SignOut() => User = null;

    /// <inheritdoc/>
    public bool CanAccessTenant(Guid tenantId) => User?.TenantRoles.ContainsKey(tenantId) == true;

    /// <inheritdoc/>
    public bool HasPermission(Guid tenantId, Permission permission) =>
        User?.TenantRoles.TryGetValue(tenantId, out var role) == true && RolePermissions.Grants(role, permission);
}
