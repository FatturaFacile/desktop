// <copyright file="LipeData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Taxation;

/// <summary>
/// Data of the Italian periodic VAT communication (Comunicazione liquidazioni periodiche IVA, model IVP18).
/// </summary>
public sealed class LipeData
{
    /// <summary>
    /// Gets the fiscal code of the taxpayer.
    /// </summary>
    public required string TaxCode { get; init; }

    /// <summary>
    /// Gets the VAT number of the taxpayer (11 digits).
    /// </summary>
    public required string VatNumber { get; init; }

    /// <summary>
    /// Gets the year.
    /// </summary>
    public required int Year { get; init; }

    /// <summary>
    /// Gets the month (monthly taxpayers).
    /// </summary>
    public int? Month { get; init; }

    /// <summary>
    /// Gets the quarter (quarterly taxpayers).
    /// </summary>
    public int? Quarter { get; init; }

    /// <summary>
    /// Gets the fiscal code of the declarant when different from the taxpayer (for example the legal representative).
    /// </summary>
    public string? DeclarantTaxCode { get; init; }

    /// <summary>
    /// Gets the role code of the declarant (1 = legal representative).
    /// </summary>
    public string DeclarantRoleCode { get; init; } = "1";

    /// <summary>
    /// Gets the total of the active operations (net of VAT).
    /// </summary>
    public decimal ActiveOperations { get; init; }

    /// <summary>
    /// Gets the total of the passive operations (net of VAT).
    /// </summary>
    public decimal PassiveOperations { get; init; }

    /// <summary>
    /// Gets the VAT due on sales.
    /// </summary>
    public decimal OutputVat { get; init; }

    /// <summary>
    /// Gets the deductible VAT on purchases.
    /// </summary>
    public decimal InputVat { get; init; }

    /// <summary>
    /// Gets the credit carried over from the previous period.
    /// </summary>
    public decimal PreviousCredit { get; init; }

    /// <summary>
    /// Gets the interest due by quarterly taxpayers (1%).
    /// </summary>
    public decimal Interest { get; init; }

    /// <summary>
    /// Gets the amount to pay (positive) or the credit (negative).
    /// </summary>
    public decimal Payable { get; init; }
}
