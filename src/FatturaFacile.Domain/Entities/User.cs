// <copyright file="User.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Represents an authenticated operator or user identity linked to a Windows domain/local account.
/// </summary>
public class User : EntityBase, ISoftDeletable
{
    /// <summary>
    /// Gets or sets the full display name of the user.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary notification email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the Windows login account name or Security Identifier (SID).
    /// </summary>
    public string WindowsUsername { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the user account is active and permitted to sign in.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <inheritdoc />
    public bool IsDeleted { get; set; }

    /// <inheritdoc />
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the collection of tenant workspaces to which this user has been granted access.
    /// </summary>
    public ICollection<TenantUser> TenantUsers { get; set; } = new List<TenantUser>();
}
