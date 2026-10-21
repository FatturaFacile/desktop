// <copyright file="PaymentMethod.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Specifies payment method.
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// Cash.
    /// </summary>
    Cash = 1,

    /// <summary>
    /// Card Sum Up Worldline.
    /// </summary>
    CardSumUpWorldline = 2,

    /// <summary>
    /// Bank Transfer.
    /// </summary>
    BankTransfer = 3,

    /// <summary>
    /// Digital Voucher.
    /// </summary>
    DigitalVoucher = 4,
}
