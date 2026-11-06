// <copyright file="ChartOfAccount.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Represents an account entry in the General Ledger Chart of Accounts (Piano dei Conti / Kontenplan).
/// Supports global statutory charts (e.g. Italian standard or Swiss SME Kontenrahmen) when TenantId is null.
/// </summary>
public class ChartOfAccount : IOptionalTenantEntity
{

    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the optional owning tenant identifier. If null, this account belongs to the standard shared chart.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the account number/code (e.g., "1000", "2000", "3000", "1.1.01").
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Gets or sets alternative account code column.
    /// </summary>
    public string? AccountCode { get; set; }

    /// <summary>
    /// Gets or sets the account title or name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account type string ("asset", "liability", "equity", "revenue", "expense").
    /// </summary>
    public string Type { get; set; } = "asset";

    /// <summary>
    /// Gets or sets the accounting standard ("it_general_ledger", "ch_sterchi_sme").
    /// </summary>
    public string Standard { get; set; } = "it_general_ledger";

    /// <summary>
    /// Gets or sets the account class.
    /// </summary>
    public string AccountClass { get; set; } = "standard";

    /// <summary>
    /// Gets or sets polymorphic account type indicator.
    /// </summary>
    public string AccountType { get; set; } = "asset";

    /// <summary>
    /// Gets or sets the normal balance direction ("debit" or "credit").
    /// </summary>
    public string NormalBalance { get; set; } = "debit";

    /// <summary>
    /// Gets or sets the currency ISO code.
    /// </summary>
    public string Currency { get; set; } = "EUR";

    /// <summary>
    /// Gets or sets a value indicating whether manual posting is permitted to this account.
    /// </summary>
    public bool AllowsManualPosting { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the soft delete timestamp.
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
