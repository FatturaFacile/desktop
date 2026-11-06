// <copyright file="OpenItemDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels.Finance;

/// <summary>
/// An open customer or supplier invoice offered for manual bank reconciliation.
/// </summary>
/// <param name="DocumentType">Invoice or PurchaseInvoice.</param>
/// <param name="Id">The document identifier.</param>
/// <param name="Number">The document number.</param>
/// <param name="Party">The customer or the supplier IBAN.</param>
/// <param name="Amount">The open amount (negative for supplier invoices).</param>
internal sealed record OpenItemDto(string DocumentType, Guid Id, string Number, string Party, decimal Amount)
{
    /// <summary>
    /// Gets the text shown in the combo box.
    /// </summary>
    public string Display => string.Create(System.Globalization.CultureInfo.CurrentCulture, $"{Number} - {Party} - {Amount:N2}");
}
