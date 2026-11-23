// <copyright file="SwissVatPurchaseLineInput.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SwissVat;

/// <summary>
/// Represents a supplier purchase line input for Swiss VAT input tax calculation.
/// </summary>
/// <param name="NetAmount">Net amount in CHF.</param>
/// <param name="InputVatAmount">Charged Swiss input VAT in CHF.</param>
/// <param name="AfcBox">Target input tax box (400 for materials/services, 405 for investments/operating expenses).</param>
/// <param name="IsDeductible">True when eligible for input tax deduction.</param>
public sealed record SwissVatPurchaseLineInput(
    decimal NetAmount,
    decimal InputVatAmount,
    string AfcBox = "400",
    bool IsDeductible = true);
