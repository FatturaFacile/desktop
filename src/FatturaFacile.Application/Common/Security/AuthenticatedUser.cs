// <copyright file="AuthenticatedUser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Common.Security;

/// <summary>
/// The signed-in user and the role held in each accessible tenant.
/// </summary>
/// <param name="UserId">The user identifier.</param>
/// <param name="UserName">The login name.</param>
/// <param name="DisplayName">The display name.</param>
/// <param name="IsSystemAdministrator">Whether the user administers all tenants.</param>
/// <param name="TenantRoles">The role per accessible tenant.</param>
public sealed record AuthenticatedUser(
    Guid UserId,
    string UserName,
    string DisplayName,
    bool IsSystemAdministrator,
    IReadOnlyDictionary<Guid, UserRole> TenantRoles);
