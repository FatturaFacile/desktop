// <copyright file="ICurrentUserContext.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Shared.Context;

/// <summary>
/// Provides contextual information regarding the currently authenticated user, active tenant, and UI language culture.
/// </summary>
public interface ICurrentUserContext
{
    /// <summary>
    /// Gets the unique identifier of the currently authenticated user, if any.
    /// </summary>
    public Guid? UserId { get; }

    /// <summary>
    /// Gets the unique identifier of the active tenant scope, if any.
    /// </summary>
    public Guid? TenantId { get; }

    /// <summary>
    /// Gets the user's selected culture code (e.g., 'en-US', 'it-CH', 'de-CH', 'fr-CH'), falling back to 'en-US'.
    /// </summary>
    public string Culture { get; }
}
