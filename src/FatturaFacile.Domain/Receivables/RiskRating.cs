// <copyright file="RiskRating.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// The rating of a composite credit risk score.
/// </summary>
public enum RiskRating
{
    /// <summary>
    /// Score below 25.
    /// </summary>
    Low = 0,

    /// <summary>
    /// Score below 50.
    /// </summary>
    Medium = 1,

    /// <summary>
    /// Score below 75.
    /// </summary>
    High = 2,

    /// <summary>
    /// Score of 75 or more.
    /// </summary>
    Critical = 3,
}
