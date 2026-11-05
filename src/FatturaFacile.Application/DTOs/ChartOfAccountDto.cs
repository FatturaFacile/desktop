// <copyright file="ChartOfAccountDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer object for an account in the chart of accounts.
/// </summary>
public sealed class ChartOfAccountDto
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the optional owning tenant identifier.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the account code.
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Gets or sets the account name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account type ("asset", "liability", "equity", "revenue", "expense").
    /// </summary>
    public string Type { get; set; } = "asset";

    /// <summary>
    /// Gets or sets the accounting standard ("it_general_ledger", "ch_sterchi_sme").
    /// </summary>
    public string Standard { get; set; } = "it_general_ledger";

    /// <summary>
    /// Gets or sets the normal balance direction ("debit", "credit").
    /// </summary>
    public string NormalBalance { get; set; } = "debit";

    /// <summary>
    /// Gets or sets the currency ISO code.
    /// </summary>
    public string Currency { get; set; } = "EUR";

    /// <summary>
    /// Gets or sets a value indicating whether manual posting is permitted.
    /// </summary>
    public bool AllowsManualPosting { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
