// <copyright file="GetAccountLedgerQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Accounting.Queries;

/// <summary>
/// Builds the account ledger of an account from the posted entries of the ledger book.
/// </summary>
/// <param name="entries">The entries repository.</param>
/// <param name="accounts">The chart of accounts repository.</param>
public sealed class GetAccountLedgerQueryHandler(IRepository<GeneralLedgerEntry> entries, IRepository<ChartOfAccount> accounts) : IRequestHandler<GetAccountLedgerQuery, AccountLedgerResult?>
{
    /// <inheritdoc/>
    public Task<AccountLedgerResult?> Handle(GetAccountLedgerQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var account = LedgerAccounts.Load(accounts).FirstOrDefault(a => a.Id == request.AccountId);
        if (account is null)
        {
            return Task.FromResult<AccountLedgerResult?>(null);
        }

        var end = request.To.Date.AddDays(1);
        var ledger = request.Ledger;
        var accountId = request.AccountId;

        var movements = entries.Query()
            .Where(e => e.IsLocked && e.DeletedAt == null && e.LedgerType == ledger && e.PostingDate < end)
            .SelectMany(e => e.Lines.Where(l => l.AccountId == accountId), (e, l) => new { e.Id, e.EntryNumber, e.PostingDate, l.Description, EntryDescription = e.Description, l.Debit, l.Credit })
            .ToList()
            .Select(static x => new AccountMovement(x.Id, x.EntryNumber, x.PostingDate, x.Description ?? x.EntryDescription, x.Debit, x.Credit));

        return Task.FromResult<AccountLedgerResult?>(AccountLedgerCalculator.Calculate(account, movements, request.From, request.To, request.Page, request.PerPage));
    }
}
