// <copyright file="AnnualRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels.VatCompliance;

/// <summary>
/// A period of the annual VAT reconciliation.
/// </summary>
/// <param name="Period">The period.</param>
/// <param name="Output">The output.</param>
/// <param name="Input">The input.</param>
/// <param name="Credit">The credit.</param>
/// <param name="Interest">The interest.</param>
/// <param name="Payable">The payable.</param>
/// <param name="Status">The status.</param>
internal sealed record AnnualRow(string Period, string Output, string Input, string Credit, string Interest, string Payable, string Status);
