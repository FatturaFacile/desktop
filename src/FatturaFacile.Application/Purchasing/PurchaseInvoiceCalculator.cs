// <copyright file="PurchaseInvoiceCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Purchasing;

/// <summary>
/// Computes the VAT and the totals of a supplier invoice from its lines.
/// </summary>
public static class PurchaseInvoiceCalculator
{
    /// <summary>
    /// Computes the line VAT (rounded to cents) and the invoice totals, including the deductible input VAT.
    /// </summary>
    /// <param name="invoice">The invoice.</param>
    /// <param name="lines">The lines.</param>
    public static void Compute(PurchaseInvoice invoice, IReadOnlyCollection<PurchaseInvoiceLine> lines)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        ArgumentNullException.ThrowIfNull(lines);

        foreach (var line in lines)
        {
            line.VatAmount = Math.Round(line.NetAmount * line.VatRate / 100m, 2, MidpointRounding.AwayFromZero);
        }

        invoice.TotalNet = lines.Sum(static l => l.NetAmount);
        invoice.TotalVat = lines.Sum(static l => l.VatAmount);
        invoice.DeductibleVat = lines.Sum(static l => Math.Round(l.VatAmount * Math.Clamp(l.DeductiblePercent, 0m, 100m) / 100m, 2, MidpointRounding.AwayFromZero));
        invoice.TotalGross = invoice.TotalNet + invoice.TotalVat;
    }
}
