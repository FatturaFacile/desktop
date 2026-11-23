// <copyright file="SwissVatReturnStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Lifecycle status of a Swiss VAT return.
/// </summary>
public enum SwissVatReturnStatus
{
    /// <summary>
    /// Draft return.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Calculated return pending quadratura validation.
    /// </summary>
    Calculated = 1,

    /// <summary>
    /// Validated against quadratura checks.
    /// </summary>
    Validated = 2,

    /// <summary>
    /// Finalized and locked against recalculation.
    /// </summary>
    Finalized = 3,

    /// <summary>
    /// Transmitted to SuisseTax.
    /// </summary>
    Submitted = 4,
}
