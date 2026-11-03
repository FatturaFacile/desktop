// <copyright file="ConstructionSiteStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Entities.Construction;

/// <summary>
/// Construction site lifecycle statuses.
/// </summary>
public static class ConstructionSiteStatus
{
    /// <summary>
    /// Active construction site.
    /// </summary>
    public const string Active = "Active";

    /// <summary>
    /// Suspended construction site.
    /// </summary>
    public const string Suspended = "Suspended";

    /// <summary>
    /// Completed construction site.
    /// </summary>
    public const string Completed = "Completed";
}
