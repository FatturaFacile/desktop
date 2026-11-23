// <copyright file="RebateExecutionMode.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// The way a settlement proposal is executed.
/// </summary>
public enum RebateExecutionMode
{
    /// <summary>
    /// A credit note is requested from the supplier.
    /// </summary>
    CreditNoteRequest = 0,

    /// <summary>
    /// An active charge invoice is issued to the supplier.
    /// </summary>
    ChargeInvoice = 1,

    /// <summary>
    /// The amount is set off against an open payable of the supplier.
    /// </summary>
    ApSetOff = 2,
}
