// <copyright file="RebateAccrualCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// Arithmetic of the periodic accrual of a supplier rebate: the qualifying spend (credit notes count negative), the accrual of a
/// period as the difference between the rebate earned to date and what was already accrued, and the journal entry it books.
/// </summary>
public static class RebateAccrualCalculator
{
    /// <summary>
    /// Signs a supplier document for the qualifying spend: a credit note (TD04) counts negative.
    /// </summary>
    /// <param name="documentType">The document type.</param>
    /// <param name="netAmount">The net amount as booked on the document.</param>
    /// <returns>The signed amount.</returns>
    public static decimal SignedNet(string documentType, decimal netAmount) =>
        string.Equals(documentType, RebateCodes.CreditNoteDocumentType, StringComparison.OrdinalIgnoreCase) ? -netAmount : netAmount;

    /// <summary>
    /// Adds up the qualifying spend of a set of supplier documents.
    /// </summary>
    /// <param name="documents">The documents.</param>
    /// <returns>The net spend, with credit notes subtracted.</returns>
    public static decimal NetVolume(IEnumerable<RebateInvoiceAmount> documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        return RebateTierCalculator.Round(documents.Sum(document => SignedNet(document.DocumentType, document.NetAmount)));
    }

    /// <summary>
    /// Computes the accrual of a period: the rebate earned to date minus what the earlier runs already accrued.
    /// </summary>
    /// <param name="earnedCumulative">The rebate earned from the start of the agreement to the end of the period.</param>
    /// <param name="alreadyAccrued">The rebate already accrued by the posted runs.</param>
    /// <returns>The accrual of the period, negative when the earned rebate dropped (for example after a credit note).</returns>
    public static decimal Delta(decimal earnedCumulative, decimal alreadyAccrued) =>
        RebateTierCalculator.Round(earnedCumulative - alreadyAccrued);

    /// <summary>
    /// Previews the journal entry of an accrual.
    /// </summary>
    /// <param name="delta">The accrual of the period.</param>
    /// <param name="hasAccounts">Whether the agreement has both its accrual and its income account.</param>
    /// <returns>The preview.</returns>
    public static RebateJournalPreview Preview(decimal delta, bool hasAccounts) =>
        new(hasAccounts && delta != 0m, Math.Abs(delta), delta >= 0m);
}
