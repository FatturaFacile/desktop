// <copyright file="PaymentMethod.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Specifies the payment mechanism used for transaction settlement and financial reconciliation.
/// </summary>
public enum PaymentMethod
{

    /// <summary>
    /// Physical currency settlement subject to statutory cash transaction threshold ceilings (e.g., Italian cash limits under Legislative Decree 231/2007).
    /// </summary>
    Cash = 1,

    /// <summary>
    /// Electronic payment card or point-of-sale (POS) terminal transaction processed via merchant gateway providers such as SumUp or Worldline.
    /// </summary>
    CardSumUpWorldline = 2,

    /// <summary>
    /// Direct interbank wire or credit transfer (e.g., SEPA Credit Transfer in the Eurozone or standard Swiss banking clearing).
    /// </summary>
    BankTransfer = 3,

    /// <summary>
    /// Digital meal voucher, corporate welfare voucher, or promotional digital token (<c>buono pasto elettronico / buono welfare</c>).
    /// </summary>
    DigitalVoucher = 4,

}
