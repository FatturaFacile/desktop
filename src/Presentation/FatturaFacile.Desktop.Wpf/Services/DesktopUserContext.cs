// <copyright file="DesktopUserContext.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Shared.Context;

namespace FatturaFacile.Desktop.Wpf.Services;

/// <summary>
/// Thread-safe in-memory desktop user session context.
/// </summary>
public sealed class DesktopUserContext : ICurrentUserContext
{
    /// <inheritdoc />
    public Guid? UserId { get; private set; }

    /// <inheritdoc />
    public Guid? TenantId { get; private set; }

    /// <inheritdoc />
    public string Culture { get; private set; } = "en";

    /// <summary>
    /// Sets authenticated user credentials and preferences.
    /// </summary>
    /// <param name="userId">The user unique identifier.</param>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <param name="culture">The UI culture code.</param>
    public void SetUser(Guid userId, Guid tenantId, string culture)
    {
        UserId = userId;
        TenantId = tenantId;
        Culture = string.IsNullOrWhiteSpace(culture) ? "en" : culture;
    }

    /// <summary>
    /// Updates the active UI culture code.
    /// </summary>
    /// <param name="culture">The new culture code.</param>
    public void SetCulture(string culture)
    {
        if (!string.IsNullOrWhiteSpace(culture))
        {
            Culture = culture;
        }
    }

    /// <summary>
    /// Clears the user session upon logout.
    /// </summary>
    public void Clear()
    {
        UserId = null;
        TenantId = null;
        Culture = "en";
    }
}
