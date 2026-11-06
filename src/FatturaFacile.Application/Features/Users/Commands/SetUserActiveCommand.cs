// <copyright file="SetUserActiveCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Users.Commands;

/// <summary>
/// Enables or disables a user (a user cannot disable itself).
/// </summary>
/// <param name="UserId">The user identifier.</param>
/// <param name="IsActive">Whether the user can sign in.</param>
public sealed record SetUserActiveCommand(Guid UserId, bool IsActive) : IRequest<bool>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.UsersManage;
}
