// <copyright file="FulfilmentOrderRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A purchase order of the order list.
/// </summary>
/// <param name="Id">The order.</param>
/// <param name="Number">The order number.</param>
/// <param name="SupplierName">The supplier.</param>
/// <param name="Status">The wire status.</param>
/// <param name="StatusText">The localized status.</param>
/// <param name="PromisedText">The promised delivery date.</param>
/// <param name="ExpectedText">The requested delivery date.</param>
/// <param name="TotalText">The total with its currency.</param>
/// <param name="RemindersText">The reminder count.</param>
/// <param name="DropshipText">The dropship flag.</param>
/// <param name="ConfirmationText">The number of the supplier confirmation.</param>
internal sealed record FulfilmentOrderRow(
    Guid Id,
    string Number,
    string SupplierName,
    string Status,
    string StatusText,
    string PromisedText,
    string ExpectedText,
    string TotalText,
    string RemindersText,
    string DropshipText,
    string ConfirmationText);
