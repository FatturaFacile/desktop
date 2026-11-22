// <copyright file="ProjectBillingSalCertificate.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectBilling;

/// <summary>
/// A SAL progress certificate (Stato Avanzamento Lavori, Akontorechnung / Teilrechnung).
/// </summary>
/// <param name="Id">The certificate identifier.</param>
/// <param name="Number">The sequential number of the certificate.</param>
/// <param name="PreviousPercent">The cumulative completion of the previous certificate.</param>
/// <param name="CumulativePercent">The cumulative completion of this certificate.</param>
/// <param name="CumulativeGross">The cumulative gross earned value.</param>
/// <param name="CurrentGross">The gross value of the period.</param>
/// <param name="AdvanceDeduction">The advance recouped.</param>
/// <param name="Retention">The guarantee retention withheld.</param>
/// <param name="InvoiceAmount">The amount to invoice.</param>
/// <param name="Net">The net amount to collect.</param>
/// <param name="IsFinal">Whether this is the final certificate.</param>
/// <param name="Status">The status of the certificate.</param>
/// <param name="PeriodStart">The first day of the period.</param>
/// <param name="PeriodEnd">The last day of the period.</param>
public sealed record ProjectBillingSalCertificate(string Id, int Number, decimal PreviousPercent, decimal CumulativePercent, decimal CumulativeGross, decimal CurrentGross, decimal AdvanceDeduction, decimal Retention, decimal InvoiceAmount, decimal Net, bool IsFinal, string Status, DateOnly? PeriodStart, DateOnly? PeriodEnd)
{
    /// <summary>
    /// Gets a value indicating whether the certificate counts: it is not cancelled.
    /// </summary>
    public bool IsLive => !string.Equals(Status, "cancelled", StringComparison.Ordinal);

    /// <summary>
    /// Gets a value indicating whether the certificate is still open (draft, pending approval or approved, not yet invoiced).
    /// </summary>
    public bool IsOpen => string.Equals(Status, "draft", StringComparison.Ordinal) || string.Equals(Status, "pending_approval", StringComparison.Ordinal) || string.Equals(Status, "approved", StringComparison.Ordinal);
}
