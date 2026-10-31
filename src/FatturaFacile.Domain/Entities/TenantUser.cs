// <copyright file="TenantUser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Represents a multi-tenant association joining a user account to a tenant workspace with assigned role and permissions.
/// </summary>
public class TenantUser : EntityBase
{
    /// <summary>
    /// Gets or sets the foreign key identifier of the tenant workspace.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property for the associated tenant organization.
    /// </summary>
    public Tenant? Tenant { get; set; }

    /// <summary>
    /// Gets or sets the foreign key identifier of the mapped user.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property for the associated user account.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets the security role assigned to the user within this specific tenant (e.g. "Admin", "Accountant", "Viewer").
    /// </summary>
    public string Role { get; set; } = "Admin";

    /// <summary>
    /// Gets or sets comma-separated or serialized permission flags assigned to this membership.
    /// </summary>
    public string? Permissions { get; set; }
}
