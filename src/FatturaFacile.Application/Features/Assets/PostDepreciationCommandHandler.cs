// <copyright file="PostDepreciationCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.Assets;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Assets;

/// <summary>
/// Handler for <see cref="PostDepreciationCommand"/>: depreciation expense in debit, accumulated depreciation in credit.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PostDepreciationCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="journal">The journal writer.</param>
public sealed class PostDepreciationCommandHandler(IUnitOfWork unitOfWork, JournalWriter journal) : IRequestHandler<PostDepreciationCommand, int>
{
    /// <inheritdoc/>
    public Task<int> Handle(PostDepreciationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.Year, cancellationToken);
    }

    private async Task<int> HandleInternalAsync(int year, CancellationToken cancellationToken)
    {
        var assets = await unitOfWork.Repository<FixedAsset>().FindAsync(static _ => true, cancellationToken).ConfigureAwait(false);
        var postings = await unitOfWork.Repository<FixedAssetDepreciation>().FindAsync(static _ => true, cancellationToken).ConfigureAwait(false);
        var expenseDefault = await journal.AccountAsync(StandardAccount.Depreciation, cancellationToken).ConfigureAwait(false);
        var accumulatedDefault = await journal.AccountAsync(StandardAccount.AccumulatedDepreciation, cancellationToken).ConfigureAwait(false);
        var count = 0;

        foreach (var asset in assets)
        {
            var history = postings.Where(p => p.FixedAssetId == asset.Id).ToList();

            if (history.Exists(p => p.Year == year))
            {
                continue;
            }

            var accumulated = history.Where(p => p.Year < year).Sum(static p => p.Amount);
            var amount = DepreciationCalculator.Amount(asset, year, accumulated);

            if (amount <= 0m)
            {
                continue;
            }

            var date = new DateTime(year, 12, 31, 0, 0, 0, DateTimeKind.Unspecified);
            var entry = await journal.WriteAsync(
                "AMM",
                date,
                $"Ammortamento {year.ToString(System.Globalization.CultureInfo.InvariantCulture)} {asset.Code} {asset.Description}",
                asset.Id,
                [(asset.DepreciationAccountId ?? expenseDefault, amount, 0m), (asset.AccumulatedAccountId ?? accumulatedDefault, 0m, amount)],
                cancellationToken).ConfigureAwait(false);

            await unitOfWork.Repository<FixedAssetDepreciation>().AddAsync(
                new FixedAssetDepreciation { FixedAssetId = asset.Id, Year = year, Amount = amount, BookValueAfter = asset.AcquisitionCost - accumulated - amount, JournalEntryId = entry.Id },
                cancellationToken).ConfigureAwait(false);
            count++;
        }

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return count;
    }
}
