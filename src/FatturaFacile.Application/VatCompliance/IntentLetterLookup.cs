// <copyright file="IntentLetterLookup.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// Loads the declarations of intent of the active tenant together with the invoices that eroded them.
/// </summary>
public static class IntentLetterLookup
{
    /// <summary>
    /// Loads the letters that are not deleted and the erosion by invoices issued here, per letter.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The letters and the sum of the usages of each.</returns>
    public static Task<(IReadOnlyList<IntentLetter> Letters, IReadOnlyDictionary<Guid, decimal> Usage)> LoadAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);

        return LoadCoreAsync(unitOfWork, cancellationToken);
    }

    /// <summary>
    /// Loads the contacts with the given identifiers.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="ids">The identifiers.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The contacts by identifier.</returns>
    public static Task<IReadOnlyDictionary<Guid, Contact>> ContactsAsync(IUnitOfWork unitOfWork, IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(ids);

        return ContactsCoreAsync(unitOfWork, [.. ids.Distinct()], cancellationToken);
    }

    private static async Task<(IReadOnlyList<IntentLetter> Letters, IReadOnlyDictionary<Guid, decimal> Usage)> LoadCoreAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        var letters = await unitOfWork.Repository<IntentLetter>().FindAsync(static l => l.DeletedAt == null, cancellationToken).ConfigureAwait(false);
        var usages = await unitOfWork.Repository<IntentLetterUsage>().FindAsync(static _ => true, cancellationToken).ConfigureAwait(false);

        return (letters, usages.GroupBy(static u => u.IntentLetterId).ToDictionary(static g => g.Key, static g => g.Sum(static u => u.Amount)));
    }

    private static async Task<IReadOnlyDictionary<Guid, Contact>> ContactsCoreAsync(IUnitOfWork unitOfWork, List<Guid> wanted, CancellationToken cancellationToken) =>
        (await unitOfWork.Repository<Contact>().FindAsync(c => wanted.Contains(c.Id), cancellationToken).ConfigureAwait(false)).ToDictionary(static c => c.Id);
}
