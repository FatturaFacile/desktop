// <copyright file="ProjectBillingSalPreview.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectBilling;

/// <summary>
/// The figures of a SAL certificate computed by <see cref="ProjectBillingSalCalculator"/>.
/// </summary>
/// <param name="CumulativeGross">The cumulative gross earned value.</param>
/// <param name="CurrentGross">The gross value of the period.</param>
/// <param name="AdvanceDeduction">The advance recouped.</param>
/// <param name="RetentionWithheld">The guarantee retention withheld.</param>
/// <param name="InvoiceAmount">The amount to invoice (gross less advance).</param>
/// <param name="NetBillable">The amount the customer pays now (invoice less retention).</param>
/// <param name="IsFinal">Whether the certificate takes the work to 100%.</param>
public sealed record ProjectBillingSalPreview(decimal CumulativeGross, decimal CurrentGross, decimal AdvanceDeduction, decimal RetentionWithheld, decimal InvoiceAmount, decimal NetBillable, bool IsFinal);
