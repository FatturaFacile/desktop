// <copyright file="IntentLetterPlafond.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// The plafond rules of the declarations of intent (art. 8 c. 1 lett. c DPR 633/1972): which letter covers an invoice, how much
/// of its plafond is left, and when the invoice must be blocked.
/// </summary>
public static class IntentLetterPlafond
{
    /// <summary>
    /// The FatturaPA nature code of an operation not subject to VAT under a declaration of intent.
    /// </summary>
    public const string Nature = "N3.5";

    private const decimal NearExhaustionShare = 0.1m;

    /// <summary>
    /// Gets the erosion of a letter. The larger of the erosion the web-api reports and the invoices issued here is taken, so that
    /// an invoice issued on another device is never forgotten and one issued here is counted before it syncs.
    /// </summary>
    /// <param name="letter">The letter.</param>
    /// <param name="localUsage">The sum of the invoices issued here.</param>
    /// <returns>The eroded amount.</returns>
    public static decimal Consumed(IntentLetter letter, decimal localUsage)
    {
        ArgumentNullException.ThrowIfNull(letter);

        return Math.Max(letter.ServerConsumedAmount, localUsage);
    }

    /// <summary>
    /// Gets the plafond left on a letter.
    /// </summary>
    /// <param name="letter">The letter.</param>
    /// <param name="localUsage">The sum of the invoices issued here.</param>
    /// <returns>The residual plafond, never below zero.</returns>
    public static decimal Residual(IntentLetter letter, decimal localUsage)
    {
        ArgumentNullException.ThrowIfNull(letter);

        return Math.Max(0m, letter.PlafondAmount - Consumed(letter, localUsage));
    }

    /// <summary>
    /// Checks whether an invoice without VAT may be issued to a customer on a date.
    /// </summary>
    /// <param name="letters">The letters of the tenant.</param>
    /// <param name="usageByLetter">The invoices issued here, summed by letter.</param>
    /// <param name="contactId">The customer.</param>
    /// <param name="date">The date of the invoice.</param>
    /// <param name="required">The amount of the lines under the declaration.</param>
    /// <returns>The outcome.</returns>
    public static PlafondCheckResult Evaluate(IReadOnlyList<IntentLetter> letters, IReadOnlyDictionary<Guid, decimal> usageByLetter, Guid contactId, DateTime date, decimal required)
    {
        ArgumentNullException.ThrowIfNull(letters);
        ArgumentNullException.ThrowIfNull(usageByLetter);

        var candidates = letters.Where(l => l.ContactId == contactId && IsValidOn(l, date.Date)).OrderBy(static l => l.ReceiptDate).ToList();
        if (candidates.Count == 0)
        {
            return Blocked(letter: null, required, 0m, VatComplianceErrors.BlockNoValidLetter);
        }

        foreach (var letter in candidates)
        {
            var residual = Residual(letter, usageByLetter.GetValueOrDefault(letter.Id));
            if (residual >= required)
            {
                return new PlafondCheckResult(
                    Allowed: true,
                    letter.Id,
                    letter.ProtocolNumber,
                    required,
                    residual,
                    BlockCode: null,
                    NearExhaustion: letter.PlafondAmount > 0m && residual - required < letter.PlafondAmount * NearExhaustionShare);
            }
        }

        var best = candidates.MaxBy(l => Residual(l, usageByLetter.GetValueOrDefault(l.Id)))!;

        return Blocked(best, required, Residual(best, usageByLetter.GetValueOrDefault(best.Id)), VatComplianceErrors.BlockPlafondExceeded);
    }

    private static bool IsValidOn(IntentLetter letter, DateTime day) => IsActiveReceived(letter) && letter.ValidFrom.Date <= day && day <= letter.ValidTo.Date;

    private static bool IsActiveReceived(IntentLetter letter) =>
        letter.Direction == IntentLetterDirection.Received
        && letter.DeletedAt is null
        && letter.SyncState != VatComplianceSyncState.PendingDelete;

    private static PlafondCheckResult Blocked(IntentLetter? letter, decimal required, decimal residual, string code) =>
        new(Allowed: false, letter?.Id, letter?.ProtocolNumber, required, residual, code, NearExhaustion: false);
}
