// <copyright file="RecentActivityItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Desktop.Wpf.ViewModels;

/// <summary>
/// Record describing a recent invoice or journal entry activity in the dashboard overview.
/// </summary>
/// <param name="InvoiceNumber">The invoice document reference number.</param>
/// <param name="Customer">The associated client or counterparty name.</param>
/// <param name="Amount">The formatted monetary transaction amount.</param>
/// <param name="Status">The current status string.</param>
/// <param name="Time">The formatted relative time representation.</param>
public sealed record RecentActivityItem(string InvoiceNumber, string Customer, string Amount, string Status, string Time);
