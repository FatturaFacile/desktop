// <copyright file="ICurrentTenantService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;

namespace FatturaFacile.Application.Ports;

/// <summary>
/// Provides access to and notifications for the actively selected tenant workspace context across the application.
/// </summary>
public interface ICurrentTenantService
{
    /// <summary>
    /// Gets or sets the unique identifier of the currently active tenant workspace.
    /// </summary>
    Guid? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the data transfer object detailing the currently active tenant organization.
    /// </summary>
    TenantDto? CurrentTenant { get; set; }

    /// <summary>
    /// Event raised whenever the active tenant workspace is changed or switched by the user.
    /// </summary>
    event EventHandler? TenantChanged;
}
