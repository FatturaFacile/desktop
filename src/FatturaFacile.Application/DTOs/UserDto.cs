// <copyright file="UserDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer object for an application user and the roles visible to the caller.
/// </summary>
public sealed class UserDto
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the login name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the linked Windows account.
    /// </summary>
    public string? WindowsAccountName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is a system administrator.
    /// </summary>
    public bool IsSystemAdministrator { get; set; }

    /// <summary>
    /// Gets or sets the role per tenant identifier (only tenants the caller manages).
    /// </summary>
    public IDictionary<Guid, string> TenantRoles { get; set; } = new Dictionary<Guid, string>();
}
