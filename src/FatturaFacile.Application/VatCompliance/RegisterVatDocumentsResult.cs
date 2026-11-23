// <copyright file="RegisterVatDocumentsResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// The outcome of the registration of documents in the VAT registers.
/// </summary>
/// <param name="SalesLines">The lines added to the sales register.</param>
/// <param name="PurchaseLines">The lines added to the purchases register.</param>
/// <param name="ProRataPercent">The pro-rata percentage applied to the purchases.</param>
public sealed record RegisterVatDocumentsResult(int SalesLines, int PurchaseLines, decimal ProRataPercent);
