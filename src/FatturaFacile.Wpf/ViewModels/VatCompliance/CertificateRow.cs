// <copyright file="CertificateRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels.VatCompliance;

/// <summary>
/// A Certificazione Unica in the grid.
/// </summary>
/// <param name="Id">The id.</param>
/// <param name="Recipient">The recipient.</param>
/// <param name="TaxCode">The tax code.</param>
/// <param name="Reason">The reason.</param>
/// <param name="Gross">The gross.</param>
/// <param name="Taxable">The taxable.</param>
/// <param name="Withheld">The withheld.</param>
/// <param name="WithheldPaid">The withheld paid.</param>
/// <param name="Status">The status.</param>
internal sealed record CertificateRow(Guid Id, string Recipient, string TaxCode, string Reason, string Gross, string Taxable, string Withheld, string WithheldPaid, string Status);
