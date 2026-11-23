// <copyright file="VatRegisterRateTotal.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// The totals of a VAT register for one rate, the riepilogo per aliquota printed under the register.
/// </summary>
/// <param name="VatRate">The VAT rate in percent.</param>
/// <param name="Nature">The nature code of a zero-rate line, when there is one.</param>
/// <param name="Taxable">The taxable amount.</param>
/// <param name="Vat">The VAT amount.</param>
/// <param name="Deductible">The deductible VAT (purchases).</param>
/// <param name="NonDeductible">The non-deductible VAT (purchases).</param>
public sealed record VatRegisterRateTotal(decimal VatRate, string? Nature, decimal Taxable, decimal Vat, decimal Deductible, decimal NonDeductible);
