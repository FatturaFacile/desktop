// <copyright file="AppUser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Application user. Users are global; their access to each tenant is granted through <see cref="UserTenantRole"/>.
/// </summary>
public class AppUser : IBaseEntity
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the unique login name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the linked Windows account (DOMAIN\user) used for Windows Hello sign-in.
    /// </summary>
    public string? WindowsAccountName { get; set; }

    /// <summary>
    /// Gets or sets the PBKDF2 password hash (null when only Windows sign-in is allowed).
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user can sign in.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the user administers the whole installation (all tenants).
    /// </summary>
    public bool IsSystemAdministrator { get; set; }

    /// <summary>
    /// Gets or sets the number of consecutive failed password sign-ins.
    /// </summary>
    public int FailedSignInCount { get; set; }

    /// <summary>
    /// Gets or sets the UTC time until which password sign-in is locked.
    /// </summary>
    public DateTime? LockoutEndUtc { get; set; }

    /// <summary>
    /// Gets or sets the UTC time of the last successful sign-in.
    /// </summary>
    public DateTime? LastSignInUtc { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the roles held in the tenants.
    /// </summary>
    public ICollection<UserTenantRole> TenantRoles { get; set; } = [];
}
