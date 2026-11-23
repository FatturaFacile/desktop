// <copyright file="RebatePayableRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// An open supplier invoice a settlement can be set off against, formatted for display.
/// </summary>
/// <param name="Id">The passive invoice identifier.</param>
/// <param name="Invoice">The invoice number.</param>
/// <param name="Date">The formatted date.</param>
/// <param name="OpenBalance">The formatted open balance.</param>
/// <param name="Covers">True when the open balance covers the gross amount of the settlement.</param>
/// <param name="Payable">The invoice behind the row.</param>
internal sealed record RebatePayableRow(string Id, string Invoice, string Date, string OpenBalance, bool Covers, RebatePayableInvoice Payable);
