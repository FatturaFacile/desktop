// <copyright file="RebatePayableInvoice.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// An open supplier invoice a settlement can be set off against.
/// </summary>
/// <param name="Id">The passive invoice identifier.</param>
/// <param name="InvoiceNumber">The invoice number.</param>
/// <param name="InvoiceDate">The invoice date.</param>
/// <param name="TotalAmount">The gross total.</param>
/// <param name="PaidAmount">What was already paid.</param>
/// <param name="Currency">The currency.</param>
public sealed record RebatePayableInvoice(string Id, string InvoiceNumber, DateOnly? InvoiceDate, decimal TotalAmount, decimal PaidAmount, string Currency)
{
    /// <summary>
    /// Gets the open balance: the total minus what was paid.
    /// </summary>
    public decimal OpenBalance => TotalAmount - PaidAmount;
}
