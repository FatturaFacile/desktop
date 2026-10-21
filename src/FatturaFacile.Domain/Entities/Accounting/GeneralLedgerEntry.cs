// <copyright file="GeneralLedgerEntry.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Accounting;

/// <summary>
/// Represents general ledger entry.
/// </summary>
public sealed class GeneralLedgerEntry : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    private readonly List<GeneralLedgerLine> lines = [];

    private GeneralLedgerEntry()
    {
    }

    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the pNL-2026/001.
    /// </summary>
    public string EntryNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the posting date.
    /// </summary>
    public DateTime PostingDate { get; private set; }

    /// <summary>
    /// Gets the description.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the currency.
    /// </summary>
    public Currency Currency { get; private set; }

    /// <summary>
    /// Gets the reference document.
    /// </summary>
    public string? ReferenceDocument { get; private set; }

    /// <summary>
    /// Gets the lines.
    /// </summary>
    public IReadOnlyCollection<GeneralLedgerLine> Lines => lines.AsReadOnly();

    /// <summary>
    /// Gets the total debit.
    /// </summary>
    public decimal TotalDebit => lines.Sum(l => l.DebitAmount);

    /// <summary>
    /// Gets the total credit.
    /// </summary>
    public decimal TotalCredit => lines.Sum(l => l.CreditAmount);

    /// <summary>
    /// Gets a value indicating whether balanced.
    /// </summary>
    public bool IsBalanced => Math.Abs(TotalDebit - TotalCredit) < 0.0001m;

    /// <summary>
    /// Gets or sets the created at utc.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the created by.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the updated at utc.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the updated by.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="entryNumber">The entry number.</param>
    /// <param name="postingDate">The posting date.</param>
    /// <param name="description">The description.</param>
    /// <param name="currency">The currency.</param>
    /// <param name="referenceDoc">The reference doc.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static GeneralLedgerEntry Create(
        Guid tenantId,
        string entryNumber,
        DateTime postingDate,
        string description,
        Currency currency,
        string? referenceDoc = null)
    {
        return new GeneralLedgerEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EntryNumber = entryNumber?.Trim() ?? string.Empty,
            PostingDate = postingDate,
            Description = description?.Trim() ?? string.Empty,
            Currency = currency,
            ReferenceDocument = referenceDoc,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Add Line.
    /// </summary>
    /// <param name="accountId">The account id.</param>
    /// <param name="debit">The debit.</param>
    /// <param name="credit">The credit.</param>
    /// <param name="lineMemo">The line memo.</param>
    public void AddLine(
        Guid accountId,
        decimal debit,
        decimal credit,
        string? lineMemo = null) =>
        lines.Add(
            new GeneralLedgerLine(
                Id,
                accountId,
                debit,
                credit,
                lineMemo));
}
