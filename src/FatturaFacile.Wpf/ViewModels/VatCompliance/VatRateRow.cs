// <copyright file="VatRateRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels.VatCompliance;

/// <summary>
/// A rate total under the VAT register grid.
/// </summary>
/// <param name="Rate">The rate.</param>
/// <param name="Taxable">The taxable.</param>
/// <param name="Vat">The vat.</param>
/// <param name="Deductible">The deductible.</param>
/// <param name="NonDeductible">The non deductible.</param>
internal sealed record VatRateRow(string Rate, string Taxable, string Vat, string Deductible, string NonDeductible);
