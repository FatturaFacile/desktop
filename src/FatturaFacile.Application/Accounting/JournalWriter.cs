// <copyright file="JournalWriter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// Writes balanced general ledger entries for the automatic postings (supplier invoices, payments, depreciation).
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="JournalWriter"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="currentTenantService">The current company.</param>
public sealed class JournalWriter(IUnitOfWork unitOfWork, ICurrentTenantService currentTenantService)
{
    /// <summary>
    /// Returns the identifier of a standard account, creating it for the company when missing.
    /// </summary>
    /// <param name="account">The standard account.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The account identifier.</returns>
    public Task<Guid> AccountAsync(StandardAccount account, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(account);

        return AccountInternalAsync(account, cancellationToken);
    }

    /// <summary>
    /// Adds a balanced, locked entry to the unit of work (not saved: the caller saves).
    /// </summary>
    /// <param name="prefix">The entry number prefix (e.g. "AC" for supplier invoices).</param>
    /// <param name="date">The posting date.</param>
    /// <param name="description">The description.</param>
    /// <param name="referenceId">The source document identifier.</param>
    /// <param name="lines">The lines: account, debit, credit.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The entry.</returns>
    /// <exception cref="InvalidOperationException">When the entry is not balanced.</exception>
    public Task<GeneralLedgerEntry> WriteAsync(
        string prefix,
        DateTime date,
        string description,
        Guid referenceId,
        IReadOnlyList<(Guid AccountId, decimal Debit, decimal Credit)> lines,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var debit = lines.Sum(static l => l.Debit);
        var credit = lines.Sum(static l => l.Credit);

        if (debit != credit)
        {
            throw new InvalidOperationException(string.Create(CultureInfo.InvariantCulture, $"Registrazione non bilanciata: dare {debit:N2}, avere {credit:N2}."));
        }

        GeneralLedgerEntry entry = new()
        {
            TenantId = currentTenantService.TenantId,
            EntryNumber = string.Create(CultureInfo.InvariantCulture, $"{prefix}-{date:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}"),
            PostingDate = date,
            DocumentDate = date,
            Description = description,
            SourceDocumentType = prefix,
            ReferenceId = referenceId,
            IsLocked = true,
        };

        var number = 1;

        foreach (var (accountId, lineDebit, lineCredit) in lines.Where(static l => l.Debit != 0m || l.Credit != 0m))
        {
            entry.Lines.Add(new GeneralLedgerLine
            {
                GeneralLedgerEntryId = entry.Id,
                AccountId = accountId,
                LineNumber = number++,
                Debit = lineDebit,
                Credit = lineCredit,
                Description = description,
            });
        }

        return AddAsync(entry, cancellationToken);
    }

    private async Task<GeneralLedgerEntry> AddAsync(GeneralLedgerEntry entry, CancellationToken cancellationToken)
    {
        await unitOfWork.Repository<GeneralLedgerEntry>().AddAsync(entry, cancellationToken).ConfigureAwait(false);
        return entry;
    }

    private async Task<Guid> AccountInternalAsync(StandardAccount account, CancellationToken cancellationToken)
    {
        var code = account.Code;
        var existing = await unitOfWork.Repository<ChartOfAccount>()
            .FindAsync(a => a.Code == code || a.AccountCode == code, cancellationToken)
            .ConfigureAwait(false);

        if (existing.FirstOrDefault() is { } found)
        {
            return found.Id;
        }

        ChartOfAccount created = new()
        {
            TenantId = currentTenantService.TenantId,
            Code = account.Code,
            AccountCode = account.Code,
            Name = account.Name,
            Type = account.AccountType,
            AccountType = account.AccountType,
            NormalBalance = account.NormalBalance,
            Standard = "shared",
            Currency = currentTenantService.DefaultCurrency,
        };

        await unitOfWork.Repository<ChartOfAccount>().AddAsync(created, cancellationToken).ConfigureAwait(false);
        return created.Id;
    }
}
