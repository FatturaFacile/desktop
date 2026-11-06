// <copyright file="SetUserPasswordCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Users.Commands;

/// <summary>
/// Sets a new password for a user and clears any lockout (administrators).
/// </summary>
public sealed class SetUserPasswordCommand : IRequest<bool>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.UsersManage;

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the new password (min. 12 characters).
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;
}
