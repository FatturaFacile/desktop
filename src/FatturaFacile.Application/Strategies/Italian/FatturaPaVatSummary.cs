// <copyright file="FatturaPaVatSummary.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Strategies.Italian;

/// <summary>
/// One FatturaPA DatiRiepilogo row: taxable amount and VAT grouped by rate and nature.
/// </summary>
/// <param name="Rate">The VAT rate in percent.</param>
/// <param name="Nature">The VAT nature code (N1..N7) for zero-rate operations, otherwise null.</param>
/// <param name="TaxableAmount">The taxable amount (ImponibileImporto).</param>
/// <param name="TaxAmount">The VAT amount (Imposta), rounded to cents on the group total.</param>
public sealed record FatturaPaVatSummary(decimal Rate, string? Nature, decimal TaxableAmount, decimal TaxAmount);
