// <copyright file="SdiInboxResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Invoicing.Sdi;

/// <summary>
/// Outcome of the processing of the files received from the SDI.
/// </summary>
/// <param name="Notifications">The processed notifications.</param>
/// <param name="PurchaseInvoices">The imported supplier invoices.</param>
/// <param name="Messages">Problems found while processing.</param>
public sealed record SdiInboxResult(int Notifications, int PurchaseInvoices, IReadOnlyList<string> Messages);
