// <copyright file="WithholdingTaxTariffStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Services;

/// <summary>
/// EF Core storage of the withholding tax tariffs with set-based deletes and batched inserts.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="WithholdingTaxTariffStore"/> class.
/// </remarks>
/// <param name="dbContext">The database context.</param>
public sealed class WithholdingTaxTariffStore(FatturaFacileDbContext dbContext) : IWithholdingTaxTariffStore
{
    /// <inheritdoc/>
    public Task<int> ReplaceAsync(IReadOnlyList<WithholdingTaxTariff> rows, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rows);

        return ReplaceInternalAsync(rows, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<WithholdingTaxTariff>> GetAsync(string canton, string tariffCode, CancellationToken cancellationToken = default)
    {
        var normalizedCanton = (canton ?? string.Empty).Trim().ToUpperInvariant();
        var normalizedCode = (tariffCode ?? string.Empty).Trim().ToUpperInvariant();

        var rows = await dbContext.WithholdingTaxTariffs
            .AsNoTracking()
            .Where(t => t.Canton == normalizedCanton && t.TariffCode == normalizedCode)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // SQLite cannot order by decimal columns: sort in memory.
        return [.. rows.OrderBy(static t => t.ValidFrom).ThenBy(static t => t.IncomeFrom)];
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<WithholdingTaxTariffSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var groups = await dbContext.WithholdingTaxTariffs
            .AsNoTracking()
            .GroupBy(static t => new { t.Canton, t.ValidFrom })
            .Select(static g => new { g.Key.Canton, g.Key.ValidFrom, Codes = g.Select(static t => t.TariffCode).Distinct().Count(), Rows = g.Count() })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var summary = groups
            .OrderBy(static g => g.Canton, StringComparer.Ordinal)
            .ThenByDescending(static g => g.ValidFrom)
            .Select(static g => new WithholdingTaxTariffSummaryDto(g.Canton, g.ValidFrom, g.Codes, g.Rows));

        return [.. summary];
    }

    private async Task<int> ReplaceInternalAsync(IReadOnlyList<WithholdingTaxTariff> rows, CancellationToken cancellationToken)
    {
        var keys = rows.Select(static r => new { r.Canton, r.ValidFrom }).Distinct().ToList();
        var autoDetect = dbContext.ChangeTracker.AutoDetectChangesEnabled;
        var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        await using (transaction.ConfigureAwait(false))
        {
            try
            {
                dbContext.ChangeTracker.AutoDetectChangesEnabled = false;

                foreach (var key in keys)
                {
                    _ = await dbContext.WithholdingTaxTariffs
                        .Where(t => t.Canton == key.Canton && t.ValidFrom == key.ValidFrom)
                        .ExecuteDeleteAsync(cancellationToken)
                        .ConfigureAwait(false);
                }

                foreach (var batch in rows.Chunk(5000))
                {
                    await dbContext.WithholdingTaxTariffs.AddRangeAsync(batch, cancellationToken).ConfigureAwait(false);
                    _ = await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    dbContext.ChangeTracker.Clear();
                }

                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                dbContext.ChangeTracker.AutoDetectChangesEnabled = autoDetect;
            }
        }

        return rows.Count;
    }
}
