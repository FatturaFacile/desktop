// <copyright file="CreateUserCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Users.Commands;

/// <summary>
/// Creates a user and optionally grants a role in the current tenant.
/// </summary>
public sealed class CreateUserCommand : IRequest<Guid>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.UsersManage;

    /// <summary>
    /// Gets or sets the unique login name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the linked Windows account (DOMAIN\user).
    /// </summary>
    public string? WindowsAccountName { get; set; }

    /// <summary>
    /// Gets or sets the initial password (min. 12 characters), or null for Windows-only sign-in.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user administers all tenants (only a system administrator may grant it).
    /// </summary>
    public bool IsSystemAdministrator { get; set; }

    /// <summary>
    /// Gets or sets the role granted in the current tenant.
    /// </summary>
    public UserRole? InitialRole { get; set; }
}
