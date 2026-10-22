// <copyright file="FilterInfoOption.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Configuration;

/// <summary>
/// Configuration model for filter item details.
/// </summary>
public sealed class FilterInfoOption
{
    /// <summary>
    /// Gets the name of the filter.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the criteria string used for filtering.
    /// </summary>
    public string FilterCriteria { get; init; } = string.Empty;

    /// <summary>
    /// Gets the URI string for the filter's icon.
    /// </summary>
    public string ImageUri { get; init; } = string.Empty;
}
