// <copyright file="ProjectBillingContractTerms.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectBilling;

/// <summary>
/// The terms of the billing contract that the SAL calculator needs.
/// </summary>
/// <param name="Model">The billing model.</param>
/// <param name="Status">The contract status.</param>
/// <param name="Jurisdiction">The fiscal jurisdiction (IT or CH).</param>
/// <param name="Currency">The currency code.</param>
/// <param name="ContractValue">The contract value.</param>
/// <param name="AdvancePercent">The advance as a percentage of the contract value.</param>
/// <param name="AdvanceAmount">The advance as a fixed amount (takes precedence over the percentage).</param>
/// <param name="AdvanceRecouped">The advance already recouped.</param>
/// <param name="RetentionPercent">The guarantee retention percentage of the contract.</param>
/// <param name="Cig">The CIG of a public contract.</param>
public sealed record ProjectBillingContractTerms(string Model, string Status, string Jurisdiction, string Currency, decimal ContractValue, decimal AdvancePercent, decimal AdvanceAmount, decimal AdvanceRecouped, decimal RetentionPercent, string? Cig)
{
    private const decimal StatutoryPublicRetentionPercent = 0.5m;

    /// <summary>
    /// Gets a value indicating whether the contract is billed by progress certificates.
    /// </summary>
    public bool BillsProgress => string.Equals(Model, "sal_progress", StringComparison.Ordinal) || string.Equals(Model, "hybrid", StringComparison.Ordinal);

    /// <summary>
    /// Gets the advance to recoup in total: the fixed amount, else the percentage of the contract value.
    /// </summary>
    public decimal AdvanceTotal => AdvanceAmount > 0 ? AdvanceAmount : Math.Round(ContractValue * AdvancePercent / 100m, 2, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Gets the retention a SAL carries: the contract's own, or the 0.50% statutory one of an Italian public contract (CIG).
    /// </summary>
    public decimal EffectiveRetentionPercent => RetentionPercent > 0 ? RetentionPercent : StatutoryRetentionPercent();

    private decimal StatutoryRetentionPercent() => IsItalianPublicContract() ? StatutoryPublicRetentionPercent : 0m;

    private bool IsItalianPublicContract() => string.Equals(Jurisdiction, "IT", StringComparison.Ordinal) && !string.IsNullOrEmpty(Cig);
}
