// <copyright file="LedgerAccounts.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// Reads the chart of accounts as the lightweight records the report calculators work on.
/// </summary>
public static class LedgerAccounts
{
    /// <summary>
    /// Loads the accounts visible to the active tenant.
    /// </summary>
    /// <param name="repository">The chart of accounts repository.</param>
    /// <returns>The accounts.</returns>
    public static IReadOnlyList<LedgerAccountInfo> Load(IRepository<ChartOfAccount> repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        return repository.Query().ToList().ConvertAll(static a => new LedgerAccountInfo(
            a.Id,
            a.Code ?? a.AccountCode ?? string.Empty,
            a.Name,
            a.AccountType,
            a.NormalBalance));
    }
}
