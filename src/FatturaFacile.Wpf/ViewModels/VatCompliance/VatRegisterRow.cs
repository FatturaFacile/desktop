// <copyright file="VatRegisterRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels.VatCompliance;

/// <summary>
/// A line of the VAT register grid.
/// </summary>
/// <param name="Protocol">The protocol.</param>
/// <param name="Date">The date.</param>
/// <param name="Document">The document.</param>
/// <param name="Partner">The partner.</param>
/// <param name="Taxable">The taxable.</param>
/// <param name="Rate">The rate.</param>
/// <param name="Vat">The vat.</param>
/// <param name="Deductible">The deductible.</param>
/// <param name="NonDeductible">The non deductible.</param>
/// <param name="State">The state.</param>
internal sealed record VatRegisterRow(string Protocol, string Date, string Document, string Partner, string Taxable, string Rate, string Vat, string Deductible, string NonDeductible, string State);
