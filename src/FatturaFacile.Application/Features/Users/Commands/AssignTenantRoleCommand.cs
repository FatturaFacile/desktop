// <copyright file="AssignTenantRoleCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Users.Commands;

/// <summary>
/// Grants or changes the role of a user in a tenant (the caller must manage users in that tenant).
/// </summary>
/// <param name="UserId">The user identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="Role">The role to grant.</param>
public sealed record AssignTenantRoleCommand(Guid UserId, Guid TenantId, UserRole Role) : IRequest<bool>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.UsersManage;
}
