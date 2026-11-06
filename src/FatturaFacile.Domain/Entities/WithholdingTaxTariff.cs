// <copyright file="WithholdingTaxTariff.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A row of the official withholding tax tariffs published by the ESTV for each canton (record 06 of the tariff files), kept by validity date.
/// </summary>
public class WithholdingTaxTariff : IBaseEntity
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the canton code (e.g. "TI").
    /// </summary>
    public string Canton { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tariff code (e.g. "A0N", "B2Y", "C1N").
    /// </summary>
    public string TariffCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first day of validity.
    /// </summary>
    public DateTime ValidFrom { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the monthly taxable income from which the row applies.
    /// </summary>
    public decimal IncomeFrom { get; set; }

    /// <summary>
    /// Gets or sets the width of the income step.
    /// </summary>
    public decimal IncomeStep { get; set; }

    /// <summary>
    /// Gets or sets the minimum tax amount.
    /// </summary>
    public decimal MinimumTax { get; set; }

    /// <summary>
    /// Gets or sets the tax rate in percent.
    /// </summary>
    public decimal RatePercent { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
