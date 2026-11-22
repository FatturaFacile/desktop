// <copyright file="ProjectBillingWip.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectBilling;

/// <summary>
/// The latest period-end work in progress valuation of a project.
/// </summary>
/// <param name="PeriodEnd">The period end date.</param>
/// <param name="Recognized">The revenue recognised to date.</param>
/// <param name="Billed">The revenue billed to date.</param>
/// <param name="Difference">Recognised less billed: a contract asset when positive, a contract liability when negative.</param>
public sealed record ProjectBillingWip(DateOnly? PeriodEnd, decimal Recognized, decimal Billed, decimal Difference)
{
    /// <summary>
    /// Gets a value indicating whether the difference is a contract asset (unbilled work).
    /// </summary>
    public bool IsContractAsset => Difference > 0;

    /// <summary>
    /// Gets a value indicating whether the difference is a contract liability (billing in excess, deferred revenue).
    /// </summary>
    public bool IsContractLiability => Difference < 0;
}
