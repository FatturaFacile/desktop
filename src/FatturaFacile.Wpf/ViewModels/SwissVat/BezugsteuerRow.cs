// <copyright file="BezugsteuerRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels.SwissVat;

/// <summary>
/// Display row for a foreign service acquisition subject to Bezugsteuer.
/// </summary>
/// <param name="Id">Entry identifier.</param>
/// <param name="InvoiceDate">Invoice date.</param>
/// <param name="Supplier">Foreign supplier name.</param>
/// <param name="Country">Supplier ISO country code.</param>
/// <param name="Category">Service category.</param>
/// <param name="NetChf">Net amount in CHF.</param>
/// <param name="TaxChf">Bezugsteuer amount in CHF.</param>
/// <param name="AfcBox">AFC box (382 or 383).</param>
/// <param name="Status">Review status.</param>
internal sealed record BezugsteuerRow(
    Guid Id,
    string InvoiceDate,
    string Supplier,
    string Country,
    string Category,
    string NetChf,
    string TaxChf,
    string AfcBox,
    string Status);
