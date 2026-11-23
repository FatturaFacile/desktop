// <copyright file="IntentLetterUsage.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// The erosion of the plafond of an intent letter by one invoice issued without VAT under art. 8 c. 1 lett. c DPR 633/1972.
/// </summary>
public class IntentLetterUsage : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the letter that covers the invoice.
    /// </summary>
    public Guid IntentLetterId { get; set; }

    /// <summary>
    /// Gets or sets the invoice.
    /// </summary>
    public Guid InvoiceId { get; set; }

    /// <summary>
    /// Gets or sets the net amount of the invoice lines without VAT.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the date of the invoice.
    /// </summary>
    public DateTime UsedAt { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the creation instant in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
