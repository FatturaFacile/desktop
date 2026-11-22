// <copyright file="GetTrialBalanceQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Accounting.Queries;

/// <summary>
/// Builds the trial balance from the posted entries of the ledger book up to the end of the period.
/// </summary>
/// <param name="entries">The entries repository.</param>
/// <param name="accounts">The chart of accounts repository.</param>
public sealed class GetTrialBalanceQueryHandler(IRepository<GeneralLedgerEntry> entries, IRepository<ChartOfAccount> accounts) : IRequestHandler<GetTrialBalanceQuery, TrialBalanceResult>
{
    /// <inheritdoc/>
    public Task<TrialBalanceResult> Handle(GetTrialBalanceQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var end = request.To.Date.AddDays(1);
        var ledger = request.Ledger;
        var costCenter = request.CostCenterId;

        var facts = entries.Query()
            .Where(e => e.IsLocked && e.DeletedAt == null && e.LedgerType == ledger && e.PostingDate < end)
            .SelectMany(e => e.Lines.Where(l => costCenter == null || l.CostCenterId == costCenter), (e, l) => new { l.AccountId, e.PostingDate, l.Debit, l.Credit })
            .ToList()
            .Select(static x => new LedgerFact(x.AccountId, x.PostingDate, x.Debit, x.Credit));

        return Task.FromResult(TrialBalanceCalculator.Calculate(LedgerAccounts.Load(accounts), facts, request.From, request.To, request.Columns));
    }
}
