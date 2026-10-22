// <copyright file="GeneralLedgerAccount.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Accounting;

/// <summary>
/// Represents a chart of accounts entry (piano dei conti / Kontenrahmen) within the double-entry bookkeeping system,
/// conforming to jurisdiction-specific accounting standards such as Swiss KMU or Italian civil code conventions.
/// </summary>
public sealed class GeneralLedgerAccount : Entity<Guid>, ITenantEntity
{
    private GeneralLedgerAccount()
    {
    }

    /// <summary>
    /// Gets or sets the unique tenant identifier for multi-tenant data partitioning.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the standardized chart of accounts code (e.g., Swiss KMU "1020" for Bank, Italian "01.01.001" for Cassa).
    /// </summary>
    public string AccountCode { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the descriptive name or label of the ledger account (e.g., "Accounts Receivable", "Crediti verso Clienti").
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the accounting classification category of this ledger account (e.g. Asset, Liability, Equity, Revenue, or Expense).
    /// </summary>
    public AccountCategory Category { get; private set; }

    /// <summary>
    /// Gets the legal or tax jurisdiction governing the chart of accounts structure (e.g. Italy or Switzerland).
    /// </summary>
    public Jurisdiction Jurisdiction { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this account is currently active and eligible for new journal postings.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Creates a new <see cref="GeneralLedgerAccount"/> with the specified code, classification, and jurisdiction.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="code">The standardized account number or code (e.g., "1020", "01.01.001").</param>
    /// <param name="name">The descriptive title of the account.</param>
    /// <param name="category">The financial statement classification category.</param>
    /// <param name="jurisdiction">The regulatory jurisdiction governing the chart of accounts.</param>
    /// <returns>A newly instantiated and active <see cref="GeneralLedgerAccount"/> entity.</returns>
    public static GeneralLedgerAccount Create(
        Guid tenantId,
        string code,
        string name,
        AccountCategory category,
        Jurisdiction jurisdiction)
    {
        return new GeneralLedgerAccount
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AccountCode = code?.Trim().ToUpperInvariant() ?? string.Empty,
            Name = name?.Trim() ?? string.Empty,
            Category = category,
            Jurisdiction = jurisdiction,
            IsActive = true,
        };
    }
}
