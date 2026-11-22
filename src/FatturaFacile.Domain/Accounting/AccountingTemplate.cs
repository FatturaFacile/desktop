// <copyright file="AccountingTemplate.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Accounting;

/// <summary>
/// A parametric journal template (causale contabile) cached from the web-api. The structured parts are kept as the JSON received.
/// </summary>
public class AccountingTemplate : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the primary key (the server identifier of the template).
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the owning tenant.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the template code (upper case, unique per tenant).
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the template category.
    /// </summary>
    public string Category { get; set; } = "general";

    /// <summary>
    /// Gets or sets the ledger book the template posts to.
    /// </summary>
    public LedgerBook LedgerType { get; set; } = LedgerBook.Statutory;

    /// <summary>
    /// Gets or sets the localized names as a JSON object keyed by language (it, de, fr, en).
    /// </summary>
    public string NamesJson { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the parameter definitions as a JSON array of key/type objects.
    /// </summary>
    public string ParametersJson { get; set; } = "[]";

    /// <summary>
    /// Gets or sets the line definitions as a JSON array of account/side/amount/description objects.
    /// </summary>
    public string LinesJson { get; set; } = "[]";

    /// <summary>
    /// Gets or sets a value indicating whether the template is a read-only system template.
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Gets or sets the instant the template was pulled from the server.
    /// </summary>
    public DateTime? SyncedAt { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
