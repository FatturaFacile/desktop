// <copyright file="DocumentSequence.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Maintains strictly sequential invoice and document number counters per tenant, document type, and fiscal year.
/// </summary>
public class DocumentSequence : EntityBase, IMustHaveTenant
{
    /// <inheritdoc />
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the document classification key (e.g. "invoice", "creditnote", "proforma").
    /// </summary>
    public string DocumentType { get; set; } = "invoice";

    /// <summary>
    /// Gets or sets the fiscal calendar year corresponding to this numbering series.
    /// </summary>
    public int FiscalYear { get; set; }

    /// <summary>
    /// Gets or sets the document prefix string (e.g. "INV-", "NC-").
    /// </summary>
    public string Prefix { get; set; } = "INV-";

    /// <summary>
    /// Gets or sets the last emitted sequential counter value.
    /// </summary>
    public int LastNumber { get; set; }

    /// <summary>
    /// Gets or sets the minimum zero-padding digit width for formatting the sequential number (defaults to 4).
    /// </summary>
    public int Padding { get; set; } = 4;
}
