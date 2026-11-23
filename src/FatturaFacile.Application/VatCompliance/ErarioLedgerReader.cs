// <copyright file="ErarioLedgerReader.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// Reads the movements of the Erario c/IVA accounts (the VAT payable account of the chart: "Erario c/IVA", "IVA dovuta") from the
/// general ledger.
/// </summary>
public static class ErarioLedgerReader
{
    /// <summary>
    /// Tells whether an account is an Erario c/IVA account.
    /// </summary>
    /// <param name="account">The account.</param>
    /// <returns>True for the VAT payable account.</returns>
    public static bool IsErarioAccount(ChartOfAccount account)
    {
        ArgumentNullException.ThrowIfNull(account);

        return HasCode(account) || IsNamedAfterErario(account.Name);
    }

    /// <summary>
    /// Reads the postings of a year on the Erario c/IVA accounts.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="year">The year.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The movements.</returns>
    public static Task<ErarioLedger> ReadAsync(IUnitOfWork unitOfWork, int year, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);

        return ReadCoreAsync(unitOfWork, year, cancellationToken);
    }

    private static bool HasCode(ChartOfAccount account) =>
        string.Equals(account.AccountCode, StandardAccount.OutputVat.Code, StringComparison.Ordinal)
        || string.Equals(account.Code, StandardAccount.OutputVat.Code, StringComparison.Ordinal);

    private static bool IsNamedAfterErario(string name) =>
        (name.Contains("erario", StringComparison.OrdinalIgnoreCase) && name.Contains("iva", StringComparison.OrdinalIgnoreCase))
        || name.Contains("iva dovuta", StringComparison.OrdinalIgnoreCase);

    private static async Task<ErarioLedger> ReadCoreAsync(IUnitOfWork unitOfWork, int year, CancellationToken cancellationToken)
    {
        var accounts = (await unitOfWork.Repository<ChartOfAccount>().FindAsync(static a => a.DeletedAt == null, cancellationToken).ConfigureAwait(false))
            .Where(IsErarioAccount)
            .Select(static a => a.Id)
            .ToList();
        if (accounts.Count == 0)
        {
            return new ErarioLedger(AccountFound: false, []);
        }

        var start = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var end = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Unspecified);
        var entries = (await unitOfWork.Repository<GeneralLedgerEntry>().FindAsync(e => e.PostingDate >= start && e.PostingDate <= end && e.DeletedAt == null, cancellationToken).ConfigureAwait(false))
            .ToDictionary(static e => e.Id);
        var ids = entries.Keys.ToList();
        var lines = await unitOfWork.Repository<GeneralLedgerLine>().FindAsync(l => accounts.Contains(l.AccountId) && ids.Contains(l.GeneralLedgerEntryId), cancellationToken).ConfigureAwait(false);

        return new ErarioLedger(AccountFound: true, [.. lines.Select(l => (entries[l.GeneralLedgerEntryId].PostingDate, l.Credit - l.Debit))]);
    }
}
