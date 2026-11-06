// <copyright file="ParsedPurchaseInvoice.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Purchasing;

/// <summary>
/// A supplier invoice read from a FatturaPA file.
/// </summary>
public sealed class ParsedPurchaseInvoice
{
    /// <summary>
    /// Gets the supplier as written in the file.
    /// </summary>
    public required Contact Supplier { get; init; }

    /// <summary>
    /// Gets the invoice header.
    /// </summary>
    public required PurchaseInvoice Invoice { get; init; }

    /// <summary>
    /// Gets the invoice lines.
    /// </summary>
    public IList<PurchaseInvoiceLine> Lines { get; } = [];

    /// <summary>
    /// Gets or sets the FatturaPA document type (TD01, TD04...).
    /// </summary>
    public string DocumentType { get; set; } = "TD01";
}
