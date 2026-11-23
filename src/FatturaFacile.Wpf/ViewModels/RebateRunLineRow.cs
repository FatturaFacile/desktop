// <copyright file="RebateRunLineRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One invoice or credit note that contributed to an accrual run, formatted for display.
/// </summary>
/// <param name="Invoice">The invoice number.</param>
/// <param name="Type">The localized document type.</param>
/// <param name="Date">The formatted date.</param>
/// <param name="Net">The formatted signed net amount.</param>
internal sealed record RebateRunLineRow(string Invoice, string Type, string Date, string Net);
