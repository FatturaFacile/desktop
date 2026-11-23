// <copyright file="VatRegisterKind.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// The Italian VAT registers kept under DPR 633/1972 (arts. 23, 24, 25 and 39).
/// </summary>
public enum VatRegisterKind
{
    /// <summary>
    /// Registro IVA vendite (art. 23).
    /// </summary>
    Sales = 0,

    /// <summary>
    /// Registro IVA acquisti (art. 25).
    /// </summary>
    Purchases = 1,

    /// <summary>
    /// Registro dei corrispettivi (art. 24).
    /// </summary>
    Receipts = 2,

    /// <summary>
    /// Registro delle merci in conto terzi (art. 39 DPR 633/1972).
    /// </summary>
    ThirdPartyGoods = 3,
}
